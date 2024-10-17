using Net.payOS.Types;

namespace Koi_Web_BE.Utils;

public interface IPayOSServices
{
    Task<CreatePaymentResult> CreateOrderAsync(int totalAmount, List<ItemData> productList);
}
