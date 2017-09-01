using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.UI.Input.Inking;
using ProtoBuf;

namespace MyWhiteboard.Stroke
{
    public class StrokeChunkManager
    {
        private const int ChunkSize = 200;
        private readonly ConcurrentDictionary<Guid, List<StrokePoints>> strokeBuffer = new ConcurrentDictionary<Guid, List<StrokePoints>>();

        public void ReceiveStrokePart(byte[] compressedStrokePoints, Action<StrokeDescription> sendFinishedStrokeAction)
        {
            using (var stream = new MemoryStream(compressedStrokePoints.Decompress()))
            {
                var strokePoints = Serializer.Deserialize<StrokePoints>(stream);
                if (strokePoints.NumberOfPackages == 1)
                {
                    sendFinishedStrokeAction((StrokeDescription)strokePoints);
                }
                strokeBuffer.AddOrUpdate(strokePoints.Id, new List<StrokePoints> { strokePoints }, (_, partsList) =>
                {
                    partsList.Add(strokePoints);
                    return partsList;
                });

                if (strokeBuffer[strokePoints.Id].Count != strokePoints.NumberOfPackages)
                {
                    return;
                }

                List<StrokePoints> parts;
                if (strokeBuffer.TryRemove(strokePoints.Id, out parts))
                {
                    parts = parts.OrderBy(p => p.Order).ToList();
                    var strokeDescription = (StrokeDescription)parts[0];

                    strokeDescription.PointXValues = parts.SelectMany(p => p.PointXValues).ToArray();
                    strokeDescription.PointYValues = parts.SelectMany(p => p.PointYValues).ToArray();
                    strokeDescription.PressureValues = parts.SelectMany(p => p.PressureValues).ToArray();
                    sendFinishedStrokeAction(strokeDescription);
                }
            }
        }

        public void SendStrokeInChunks(Guid strokeId, List<InkPoint> points, InkDrawingAttributes drawingAttributes, Action<byte[]> sendAction)
        {
            var isFirstChunk = true;
            var chunksOfPoints = ChunksOfPoints(points, ChunkSize).ToList();

            for (short i = 0; i < chunksOfPoints.Count; i++)
            {
                var chunk = chunksOfPoints[i];
                StrokePoints strokePoints;
                if (isFirstChunk)
                {
                    strokePoints = new StrokeDescription
                    {
                        IsPencil = drawingAttributes.Kind == InkDrawingAttributesKind.Pencil,
                        DrawAsHighlighter = drawingAttributes.DrawAsHighlighter,
                        ColorValues = new[] { drawingAttributes.Color.A, drawingAttributes.Color.R, drawingAttributes.Color.G, drawingAttributes.Color.B },
                        FitToCurve = drawingAttributes.FitToCurve,
                        IgnorePressure = drawingAttributes.IgnorePressure,
                        SizeValues = new[] { drawingAttributes.Size.Width, drawingAttributes.Size.Height },
                        Opacity = drawingAttributes.PencilProperties?.Opacity ?? 1.0
                    };
                    isFirstChunk = false;
                }
                else
                {
                    strokePoints = new StrokePoints();
                }
                strokePoints.Id = strokeId;
                strokePoints.Order = i;
                strokePoints.NumberOfPackages = (short)chunksOfPoints.Count;

                strokePoints.PointXValues = chunk.Select(p => p.Position.X).ToArray();
                strokePoints.PointYValues = chunk.Select(p => p.Position.Y).ToArray();
                strokePoints.PressureValues = chunk.Select(p => p.Pressure).ToArray();

                byte[] protobuf;
                using (var stream = new MemoryStream())
                {
                    Serializer.Serialize(stream, strokePoints);
                    protobuf = stream.ToArray();
                }

                var compressedProtobuf = protobuf.Compress();

                sendAction(compressedProtobuf);
            }
        }

        //public byte[] GetStrokeBytes(StrokeDescription strokeDescription)
        //{
            
        //}

        private IEnumerable<List<InkPoint>> ChunksOfPoints(List<InkPoint> sourcePoints, int chunkSize)
        {
            var points = new List<InkPoint>(chunkSize);
            for (var i = 0; i < sourcePoints.Count; i++)
            {
                if (points.Count > 0 && i % chunkSize == 0)
                {
                    yield return points;
                    points = new List<InkPoint>(chunkSize);
                }

                points.Add(sourcePoints[i]);
            }

            if (points.Count > 0)
            {
                yield return points;
            }
        }
    }
}