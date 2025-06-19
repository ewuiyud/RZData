using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Newtonsoft.Json;
using RZData.Models;
using RZData.Services;
using RZData.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebService;

namespace RZData.Tools
{
    public class AIMatchTool
    {
        public static string AIMatchForSystemFamily(string categoryName, string familyName, string extendName)
        {
            string matchResult = "";
            List<ExcelFamilyModel> matchList = new List<ExcelFamilyModel>();
            matchList = ExcelDataService.ExcelFamilyRecords.FindAll(a => a.FamilyCategory == categoryName && familyName == a.FamilyName);
            string listString = "";
            foreach (var item in matchList)
            {
                listString += string.Format($"{item.FamilyCategory}-{item.FamilyName}-{item.ExtendName}\n\t");
            }
            string ak = "sk-93b2d8e03fb04919a89aa235923a7fd0";
            var dp = new DeepSeek(ak);
            var userPromt = new[] { $"待分类数据为：{categoryName}-{familyName}-{extendName}\n" +
                $"分类表内容为{listString}" };
            string systemPromt = FileTool.ReadFileContent("RZData.SystemPromt.MatchNameSystemPromt.txt");
            dp.Chat(userPromt, systemPromt);
            if (!string.IsNullOrEmpty(dp.ErrorMessage))
            {
                matchResult = string.Format("ErrorMessage:{0}", dp.ErrorMessage);
            }
            else
            {
                matchResult = dp.ResultJson.choices[0].message.content;
            }
            return matchResult;
        }
    }
}
