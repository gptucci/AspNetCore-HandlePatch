namespace PatchHanlde.JsonPatch
{
    [Flags]
    public enum PathFlags
    {
        None = 0,
        CanWrite = 1,
        IsArray = 2,
        IsCollection = 4,
        IsObject = 8,
    }
}
