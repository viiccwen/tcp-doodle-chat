using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace WindowsFormsApp1.Managers
{
    public enum Tool
    {
        ConnectServer,
        DisconnectServer,
        UpdateUserList,
        SendMessage,
        SendPrivateMessage,
        PrivatePencil,
        PrivateEraser,
        PublicPencil,
        PublicEraser,
        Clear
    }

    public class DrawingManager
    {
        private Bitmap canvasBitmap;
        public Graphics canvasGraphics;
        private Tool currentTool = Tool.PublicPencil;
        private Color currentColor = Color.Black;
        private int penSize = 3;
        private readonly Stack<Bitmap> undoStack = new Stack<Bitmap>();
        private readonly Stack<Bitmap> redoStack = new Stack<Bitmap>();

        public event Action OnStackChanged;

        private Point previousPoint;
        private Point shapeStart;
        private Point shapeEnd;

        public Bitmap Canvas => canvasBitmap;
        public int UndoStackCount => undoStack.Count;
        public int RedoStackCount => redoStack.Count;
        public bool IsDrawing { get; set; }
        public Tool CurrentTool
        {
            get => currentTool;
            set => currentTool = value;
        }
        public Color CurrentColor
        {
            get => currentColor;
            set => currentColor = value;
        }
        public int PenSize
        {
            get => penSize;
            set => penSize = value;
        }
        public Point PreviousPoint
        {
            get => previousPoint;
            set => previousPoint = value;
        }
        public Point ShapeStart
        {
            get => shapeStart;
            set => shapeStart = value;
        }
        public Point ShapeEnd
        {
            get => shapeEnd;
            set => shapeEnd = value;
        }

        public DrawingManager(int width, int height)
        {
            canvasBitmap = new Bitmap(width, height);
            canvasGraphics = Graphics.FromImage(canvasBitmap);
            canvasGraphics.Clear(Color.White);
        }

        public void SaveUndo()
        {
            undoStack.Push((Bitmap)canvasBitmap.Clone());
            redoStack.Clear();
            OnStackChanged?.Invoke();
        }

        public void Undo()
        {
            if (undoStack.Count > 0)
            {
                redoStack.Push((Bitmap)canvasBitmap.Clone());
                canvasBitmap = undoStack.Pop();
                canvasGraphics = Graphics.FromImage(canvasBitmap);
                OnStackChanged?.Invoke();
            }
        }

        public void Redo()
        {
            if (redoStack.Count > 0)
            {
                undoStack.Push((Bitmap)canvasBitmap.Clone());
                canvasBitmap = redoStack.Pop();
                canvasGraphics = Graphics.FromImage(canvasBitmap);
                OnStackChanged?.Invoke();
            }
        }

        public void DrawPencil(Point end)
        {
            if (PreviousPoint != end)
            {
                using (Pen pen = new Pen(currentTool == Tool.PublicPencil || currentTool == Tool.PrivatePencil ? currentColor : Color.White, penSize))
                {
                    pen.StartCap = pen.EndCap = LineCap.Round;
                    canvasGraphics.DrawLine(pen, PreviousPoint, end);
                }
            }
            previousPoint = end;
        }

        public void Clear()
        {
            canvasGraphics.Clear(Color.White);
        }

        public void ApplyRemoteDrawAction(DrawAction action)
        {
            switch (action.Tool)
            {
                // It's the same action with eraser.
                case Tool.PublicPencil:
                case Tool.PrivatePencil:
                case Tool.PrivateEraser:
                case Tool.PublicEraser:
                    CurrentTool = action.Tool;
                    CurrentColor = action.Color;
                    PenSize = action.PenSize;
                    PreviousPoint = action.Start;
                    DrawPencil(action.End);
                    break;
                case Tool.Clear:
                    Clear();
                    break;
            }
        }

        // set tool 
        public void SetTool(Tool tool) => currentTool = tool;
        public void SetColor(Color color) => currentColor = color;
        public void SetPenSize(int size) => penSize = size;
        public void SetPreviousPoint(Point point) => previousPoint = point;
        public void SetShapeStart(Point point) => shapeStart = point;
        public void SetShapeEnd(Point point) => shapeEnd = point;
    }
}
