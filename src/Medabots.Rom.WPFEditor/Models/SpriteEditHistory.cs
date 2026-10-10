namespace Medabots.Rom.WPFEditor.Models;

public sealed class SpriteEditHistory
{
    private readonly Stack<SpriteEditSnapshot> _undoStack = new();
    private readonly Stack<SpriteEditSnapshot> _redoStack = new();

    public bool CanUndo => _undoStack.Count > 0;

    public bool CanRedo => _redoStack.Count > 0;

    // A new edit: the undone steps can no longer be redone.
    public void Push(byte[] pixels, byte[] palette)
    {
        _undoStack.Push(new SpriteEditSnapshot([new SpriteEditImageSnapshot(pixels, palette)]));
        _redoStack.Clear();
    }

    public void Push(IEnumerable<(byte[] Pixels, byte[] Palette)> images)
    {
        _undoStack.Push(new SpriteEditSnapshot(images.Select(image => new SpriteEditImageSnapshot(image.Pixels, image.Palette)).ToArray()));
        _redoStack.Clear();
    }

    public void PushRedo(SpriteEditSnapshot snapshot) => _redoStack.Push(snapshot);

    public SpriteEditSnapshot PopRedo() => _redoStack.Pop();

    public void PushUndoKeepingRedo(SpriteEditSnapshot snapshot) => _undoStack.Push(snapshot);

    public SpriteEditSnapshot Pop()
    {
        return _undoStack.Pop();
    }

    public void Clear()
    {
        _undoStack.Clear();
        _redoStack.Clear();
    }
}

public sealed record SpriteEditSnapshot(IReadOnlyList<SpriteEditImageSnapshot> Images);

public sealed record SpriteEditImageSnapshot
{
    public SpriteEditImageSnapshot(byte[] pixels, byte[] palette)
    {
        Pixels = pixels.ToArray();
        Palette = palette.ToArray();
    }

    public byte[] Pixels { get; }

    public byte[] Palette { get; }
}
