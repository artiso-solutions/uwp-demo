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

namespace ImageReview.Stroke
{
    public class StrokeSynchronization
    {
        private readonly InkCanvas canvas;
        private readonly InkToolbar inkToolbar;
        private readonly object canvasChangeLock = new object();
        private readonly Dictionary<Guid, InkStroke> idToStrokeMapping;
        private readonly InkStrokeBuilder strokeBuilder;
        private readonly StrokeChangeBroker strokeChangeBroker;
        private readonly Dictionary<InkStroke, Guid> strokeToIdMapping;

        public StrokeSynchronization(InkCanvas canvas, InkToolbar inkToolbar, StrokeChangeBroker strokeChangeBroker)
        {
            this.canvas = canvas;
            this.inkToolbar = inkToolbar;
            this.strokeChangeBroker = strokeChangeBroker;

            strokeBuilder = new InkStrokeBuilder();

            strokeToIdMapping = new Dictionary<InkStroke, Guid>();
            idToStrokeMapping = new Dictionary<Guid, InkStroke>();

            strokeChangeBroker.StrokeCollected += StrokeChangeBrokerOnStrokeCollected;
            strokeChangeBroker.StrokeErased += StrokeChangeBrokerOnStrokeErased;
            strokeChangeBroker.AllStrokeErased += StrokeChangeBrokerOnAllStrokeErased;

            inkToolbar.EraseAllClicked += InkToolbarOnEraseAllClicked;

            canvas.InkPresenter.StrokesCollected += InkPresenterOnStrokesCollected;
            canvas.InkPresenter.StrokesErased += InkPresenterOnStrokesErased;
        }

        public void StopSynchronization()
        {
            strokeChangeBroker.StrokeCollected -= StrokeChangeBrokerOnStrokeCollected;
            strokeChangeBroker.StrokeErased -= StrokeChangeBrokerOnStrokeErased;
            strokeChangeBroker.AllStrokeErased -= StrokeChangeBrokerOnAllStrokeErased;

            inkToolbar.EraseAllClicked -= InkToolbarOnEraseAllClicked;

            canvas.InkPresenter.StrokesCollected -= InkPresenterOnStrokesCollected;
            canvas.InkPresenter.StrokesErased -= InkPresenterOnStrokesErased;
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
                        strokeToIdMapping[newStroke] = strokeDescription.Id;

                        canvas.InkPresenter.StrokeContainer.AddStroke(newStroke);
                    }
                });
        }

        private async void StrokeChangeBrokerOnStrokeErased(object sender, Guid strokeId)
        {
            if (!idToStrokeMapping.ContainsKey(strokeId))
            {
                return;
            }

            var stroke = idToStrokeMapping[strokeId];
            idToStrokeMapping.Remove(strokeId);
            strokeToIdMapping.Remove(stroke);

            foreach (var strokeMapping in strokeToIdMapping.ToList())
            {
                var newStroke = strokeMapping.Key.Clone();
                strokeToIdMapping.Remove(strokeMapping.Key);
                strokeToIdMapping[newStroke] = strokeMapping.Value;
                idToStrokeMapping[strokeMapping.Value] = newStroke;
            }

            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    lock (canvasChangeLock)
                    {
                        canvas.InkPresenter.StrokeContainer.Clear();
                        canvas.InkPresenter.StrokeContainer.AddStrokes(idToStrokeMapping.Values);
                    }
                });
        }

        private async void StrokeChangeBrokerOnAllStrokeErased(object sender, EventArgs eventArgs)
        {
            strokeToIdMapping.Clear();
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

        private void InkToolbarOnEraseAllClicked(InkToolbar sender, object args)
        {
            strokeChangeBroker.SendEraseAllStrokes();

            strokeToIdMapping.Clear();
            idToStrokeMapping.Clear();
        }

        private void InkPresenterOnStrokesCollected(InkPresenter sender, InkStrokesCollectedEventArgs args)
        {
            foreach (var stroke in args.Strokes)
            {
                var strokeId = Guid.NewGuid();

                strokeToIdMapping[stroke] = strokeId;
                idToStrokeMapping[strokeId] = stroke;


                strokeChangeBroker.SendStrokeCollected(strokeId, stroke);
            }
        }

        private void InkPresenterOnStrokesErased(InkPresenter sender, InkStrokesErasedEventArgs args)
        {
            foreach (var stroke in args.Strokes)
            {
                var id = strokeToIdMapping[stroke];
                strokeChangeBroker.SendEraseStroke(id);

                strokeToIdMapping.Remove(stroke);
                idToStrokeMapping.Remove(id);
            }
        }
    }
}