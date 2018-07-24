using Zealot.Common;
using Zealot.Repository;

namespace Zealot.Server.Inventory
{
    public class ServerItemFactory : BaseItemFactory
    {
        public override IInventoryItem CreateItemInstance(int itemid)
        {
            IInventoryItem retItem = GetInventoryItem(itemid);

            //TODO: check itemtype and do additional handling, e.g. UID generation
            

            return retItem;
        }
    }
}
