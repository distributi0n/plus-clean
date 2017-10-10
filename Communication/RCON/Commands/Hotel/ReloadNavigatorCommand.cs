namespace Plus.Communication.RCON.Commands.Hotel
{
    internal class ReloadNavigatorCommand : IRCONCommand
    {
        public string Description => "This command is used to reload the navigator.";

        public string Parameters => "";

        public bool TryExecute(string[] parameters)
        {
            PlusEnvironment.GetGame().GetNavigator().Init();
            return true;
        }
    }
}