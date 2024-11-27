using System.Collections.Generic;

namespace NutriDbService.PythModels.Request
{
    public class AddUserExtraRequest
    {
        public long UserTgId { get; set; }
        public Dictionary<string,string> Info { get; set; }
    }
}
