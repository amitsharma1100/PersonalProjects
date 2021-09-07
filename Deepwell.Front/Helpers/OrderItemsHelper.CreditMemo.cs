using Deepwell.Common.CommonModel.Order;
using Deepwell.Common.Extensions;
using Deepwell.Front.Models.Order;
using System.Collections.Generic;
using System.Linq;

namespace Deepwell.Front.Helpers
{
    public static partial class OrderItemsHelper
    {
        private const string CREDIT_MEMO_ITEMS_KEY = "CreditMemoItems";

        public static List<CreditMemoItemModel> CreditMemoItems
        {
            get
            {
                var creditMemoItems = sessionState[CREDIT_MEMO_ITEMS_KEY];
                if (creditMemoItems.IsNull())
                {
                    creditMemoItems = new List<CreditMemoItemModel>();
                }

                return (creditMemoItems as List<CreditMemoItemModel>).OrderBy(i => i.ProductId).ToList();
            }
            set
            {
                sessionState[CREDIT_MEMO_ITEMS_KEY] = value;
            }
        }

        public static void AddCreditMemoItem(CreditMemoItemModel item)
        {
            if (CreditMemoItems.Where(cm => cm.ProductId == item.ProductId).Any() == false)
            {
                var oldList = CreditMemoItems;
                oldList.Add(item);
                CreditMemoItems = oldList;
            }
        }

        public static void EmptyCreditMemoSessionItems()
        {
            CreditMemoItems = new List<CreditMemoItemModel>();
        }
    }
}