using CQRS.Core.Messages;

namespace CQRS.Core.Commands
{
    public abstract class BaseCommand : Message
    {
        // Without properties, we just need every Command to have at leas Id field
    }
}