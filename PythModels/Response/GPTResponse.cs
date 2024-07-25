using System.Collections.Generic;

namespace NutriDbService.PythModels.Response
{
    public class GPTResponse
    {
        public string pretty { get; set; }

        public List<PythFood> food { get; set; }
    }
}
