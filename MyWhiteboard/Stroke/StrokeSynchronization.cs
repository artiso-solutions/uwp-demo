using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml.Controls;

namespace MyWhiteboard.Stroke
{
    public class StrokeSynchronization
    {
        private readonly InkCanvas canvas;
        private readonly InkToolbar inkToolbar;
        private readonly object canvasChangeLock = new object();
        private readonly Dictionary<Guid, InkStroke> idToStrokeMapping;
        private readonly InkStrokeBuilder strokeBuilder;
        private readonly StrokeChangeBroker strokeChangeBroker;

        public StrokeSynchronization(InkCanvas canvas, InkToolbar inkToolbar, StrokeChangeBroker strokeChangeBroker)
        {
            this.canvas = canvas;
            this.inkToolbar = inkToolbar;
            this.strokeChangeBroker = strokeChangeBroker;

            strokeBuilder = new InkStrokeBuilder();

            idToStrokeMapping = new Dictionary<Guid, InkStroke>();

            strokeChangeBroker.StrokeCollected += StrokeChangeBrokerOnStrokeCollected;
            strokeChangeBroker.StrokeErased += StrokeChangeBrokerOnStrokeErased;
            strokeChangeBroker.AllStrokeErased += StrokeChangeBrokerOnAllStrokeErased;
            strokeChangeBroker.ResendAllStrokesRequested += StrokeChangeBrokerOnResendAllStrokesRequested;

            inkToolbar.EraseAllClicked += InkToolbarOnEraseAllClicked;

            canvas.InkPresenter.StrokesCollected += InkPresenterOnStrokesCollected;
            canvas.InkPresenter.StrokesErased += InkPresenterOnStrokesErased;
        }

        public void StopSynchronization()
        {
            strokeChangeBroker.StrokeCollected -= StrokeChangeBrokerOnStrokeCollected;
            strokeChangeBroker.StrokeErased -= StrokeChangeBrokerOnStrokeErased;
            strokeChangeBroker.AllStrokeErased -= StrokeChangeBrokerOnAllStrokeErased;
            strokeChangeBroker.ResendAllStrokesRequested -= StrokeChangeBrokerOnResendAllStrokesRequested;

            inkToolbar.EraseAllClicked -= InkToolbarOnEraseAllClicked;

            canvas.InkPresenter.StrokesCollected -= InkPresenterOnStrokesCollected;
            canvas.InkPresenter.StrokesErased -= InkPresenterOnStrokesErased;

            strokeChangeBroker.StopBroker();
        }

        private async void StrokeChangeBrokerOnStrokeCollected(object sender, StrokeDescription strokeDescription)
        {
            if (idToStrokeMapping.ContainsKey(strokeDescription.Id))
            {
                return;
            }

            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    lock (canvasChangeLock)
                    {
                        InkDrawingAttributes inkDrawingAttributes;
                        if (strokeDescription.IsPencil)
                        {
                            inkDrawingAttributes = InkDrawingAttributes.CreateForPencil();
                            inkDrawingAttributes.PencilProperties.Opacity = strokeDescription.Opacity;
                        }
                        else
                        {
                            inkDrawingAttributes = new InkDrawingAttributes();
                            inkDrawingAttributes.DrawAsHighlighter = strokeDescription.DrawAsHighlighter;
                        }

                        inkDrawingAttributes.Color = Color.FromArgb(strokeDescription.ColorValues[0], strokeDescription.ColorValues[1], strokeDescription.ColorValues[2], strokeDescription.ColorValues[3]);
                        inkDrawingAttributes.FitToCurve = strokeDescription.FitToCurve;
                        inkDrawingAttributes.IgnorePressure = strokeDescription.IgnorePressure;
                        inkDrawingAttributes.Size = new Size(strokeDescription.SizeValues[0], strokeDescription.SizeValues[1]);

                        strokeBuilder.SetDefaultDrawingAttributes(inkDrawingAttributes);

                        var points = new List<InkPoint>(strokeDescription.PointXValues.Length);
                        for (var i = 0; i < strokeDescription.PointXValues.Length; i++)
                        {
                            points.Add(new InkPoint(new Point(strokeDescription.PointXValues[i], strokeDescription.PointYValues[i]), strokeDescription.PressureValues[i]));
                        }

                        var newStroke = strokeBuilder.CreateStrokeFromInkPoints(points, Matrix3x2.Identity);
                        idToStrokeMapping[strokeDescription.Id] = newStroke;

                        canvas.InkPresenter.StrokeContainer.AddStroke(newStroke);
                    }
                });
        }

        private async void StrokeChangeBrokerOnStrokeErased(object sender, Guid strokeId)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    lock (canvasChangeLock)
                    {
                        if (!idToStrokeMapping.ContainsKey(strokeId))
                        {
                            return;
                        }

                        idToStrokeMapping.Remove(strokeId);

                        foreach (var strokeMapping in idToStrokeMapping.ToList())
                        {
                            var newStroke = strokeMapping.Value.Clone();
                            idToStrokeMapping[strokeMapping.Key] = newStroke;
                        }

                        canvas.InkPresenter.StrokeContainer.Clear();
                        canvas.InkPresenter.StrokeContainer.AddStrokes(idToStrokeMapping.Values.ToList());
                    }
                });
        }

        private async void StrokeChangeBrokerOnAllStrokeErased(object sender, EventArgs eventArgs)
        {
            idToStrokeMapping.Clear();

            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    lock (canvasChangeLock)
                    {
                        canvas.InkPresenter.StrokeContainer.Clear();
                    }
                });
        }

        private void StrokeChangeBrokerOnResendAllStrokesRequested(object sender, EventArgs eventArgs)
        {
            foreach (var inkStroke in idToStrokeMapping)
            {
                strokeChangeBroker.SendStrokeCollected(inkStroke.Key, inkStroke.Value);
            }
        }

        private void InkToolbarOnEraseAllClicked(InkToolbar sender, object args)
        {
            idToStrokeMapping.Clear();

            strokeChangeBroker.SendEraseAllStrokes();
        }

        private void InkPresenterOnStrokesCollected(InkPresenter sender, InkStrokesCollectedEventArgs args)
        {
            foreach (var stroke in args.Strokes)
            {
                var strokeId = Guid.NewGuid();

                idToStrokeMapping[strokeId] = stroke;

                strokeChangeBroker.SendStrokeCollected(strokeId, stroke);
            }
        }

        private void InkPresenterOnStrokesErased(InkPresenter sender, InkStrokesErasedEventArgs args)
        {
            foreach (var stroke in args.Strokes)
            {
                foreach (var inkStrokeMapping in idToStrokeMapping.ToList())
                {
                    if (inkStrokeMapping.Value != stroke)
                    {
                        continue;
                    }

                    var id = inkStrokeMapping.Key;
                    idToStrokeMapping.Remove(id);
                    strokeChangeBroker.SendEraseStroke(id);
                }
            }
        }
    }
}