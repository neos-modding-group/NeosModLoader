namespace NeosModLoader
{
    public abstract class NeosMod
    {
        public static void Debug(string message) => Logger.DebugExternal(message);
        public static void Msg(string message) => Logger.MsgExternal(message);
        public static void Warn(string message) => Logger.WarnExternal(message);
        public static void Error(string message) => Logger.ErrorExternal(message);
        public virtual void OnEngineInit() { }
        public abstract string Name { get; }
        public abstract string Author { get; }
        public abstract string Version { get; }
        public virtual string Link { get; }
    }
}
