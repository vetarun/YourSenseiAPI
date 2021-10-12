using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using YourSensei.ViewModel.Subscription;

namespace YourSensei.Utility
{
    public class EnumHelper
    {
        public static string GetDescription(Enum en)
        {
            Type type = en.GetType();
            MemberInfo[] memInfo = type.GetMember(en.ToString());

            if (memInfo != null && memInfo.Length > 0)
            {
                object[] attrs = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (attrs != null && attrs.Length > 0)
                {
                    return ((DescriptionAttribute)attrs[0]).Description;
                }
            }

            return en.ToString();
        }

        public static async Task<List<FeaturesAllowedModel>> GetDescriptions(Type type)
        {
            List < FeaturesAllowedModel > descs = new List<FeaturesAllowedModel>();
            var names = Enum.GetNames(type);
            foreach (var name in names)
            {
                var field = type.GetField(name);
                var fds = field.GetCustomAttributes(typeof(DescriptionAttribute), true);
                foreach (DescriptionAttribute fd in fds)
                {
                  await Task.Run(()=> descs.Add(new FeaturesAllowedModel { Key=name,Value= fd.Description }));
                }
            }
            return descs;
        }
    }
}
