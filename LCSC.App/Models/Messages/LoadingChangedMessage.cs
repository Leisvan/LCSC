using CommunityToolkit.Mvvm.Messaging.Messages;

namespace LCSC.App.Models.Messages;

public class LoadingChangedMessage(bool value) : ValueChangedMessage<bool>(value)
{
}