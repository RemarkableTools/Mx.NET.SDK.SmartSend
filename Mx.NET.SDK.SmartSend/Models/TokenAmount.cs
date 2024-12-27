using Mx.NET.SDK.Core.Domain.Values;
using Mx.NET.SDK.Core.Domain;
using System.Globalization;

namespace Mx.NET.SDK.SmartSend.Models
{
    public abstract class TokenAmount
    {
        public Address Address { get; set; }
        public ESDTAmount Amount { get; set; }

        protected TokenAmount(string address, decimal amount, ESDT esdt)
        {
            Address = Address.FromBech32(address);
            Amount = ESDTAmount.ESDT(amount.ToString(CultureInfo.InvariantCulture), esdt);
        }
    }
}
