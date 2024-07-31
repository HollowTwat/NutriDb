using System.Collections.Generic;

namespace NutriDbService.PythModels.Request
{
    public class AddUserExtraRequest
    {
        public int UserTgId { get; set; }
        public Dictionary<string,string> Info { get; set; }
    }
}
