using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RZData.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RZData.Services
{
    /// <summary>
    /// Revit属性管理服务
    /// </summary>
    public class RevitPropertyManager
    {
        private readonly UIDocument _uIDocument;

        public RevitPropertyManager(UIDocument uIDocument)
        {
            _uIDocument = uIDocument;
        }

        /// <summary>
        /// 为族添加实例属性
        /// </summary>
        public void AddPropertiesToFamilies(List<InsetParameterData> familyData)
        {
            using (Transaction trans = new Transaction(_uIDocument.Document, "添加族实例属性"))
            {
                trans.Start();

                try
                {
                    Dictionary<string, List<string>> addedParameters = new Dictionary<string, List<string>>();
                    foreach (var data in familyData)
                    {
                        CreateProjectParameter(_uIDocument, "cailia", BuiltInCategory.OST_Walls, ParameterType.Text);
                    }

                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans.RollBack();
                    throw new Exception($"添加属性失败: {ex.Message}", ex);
                }
            }
        }


        /// <summary>
        /// 为指定类型的元素创建项目参数
        /// </summary>
        /// <param name="uidoc">文档</param>
        /// <param name="parameterName">参数名称</param>
        /// <param name="builtInCategory">指定元素的类别</param>
        /// <param name="parameterType">参数类型</param>
        public void CreateProjectParameter(
            UIDocument uidoc,
            string parameterName,
            BuiltInCategory builtInCategory,
            ParameterType parameterType)
        {
            Document doc = uidoc.Document;
            Autodesk.Revit.ApplicationServices.Application app = uidoc.Application.Application;
            // 1.
            string filePath = "MySharedParameterFile.txt";
            FileStream fs = File.Create(filePath);
            fs.Close();
            // 2.
            app.SharedParametersFilename = filePath;
            // 3.
            DefinitionFile definitionFile = app.OpenSharedParameterFile();

            // 4. 共享参数创建
            var group = definitionFile.Groups.get_Item("Group") ?? definitionFile.Groups.Create("Group");
            Definition definition = group.Definitions.get_Item(parameterName);
            if (definition == null)
            {
                ExternalDefinitionCreationOptions edco = new ExternalDefinitionCreationOptions(parameterName, parameterType);
                definition = group.Definitions.Create(edco);
            }

            // 5.
            CategorySet categories = app.Create.NewCategorySet();
            Category category = doc.Settings.Categories.get_Item(builtInCategory);
            categories.Insert(category);

            // 6. 
            ElementBinding binding = app.Create.NewInstanceBinding(categories); //  new InstanceBinding(categories);
                                                                                //ElementBinding binding = app.Create.NewTypeBinding(categories);

            // 7. 项目参数绑定
            BindingMap bingingMap = doc.ParameterBindings;
            bingingMap.Insert(definition, binding);

            doc.Regenerate();

            //definitionFile.Dispose();
            //File.Delete(filePath);
        }

        /// <summary>
        /// 获取族实例统计信息
        /// </summary>
        public Dictionary<string, int> GetFamilyInstanceCounts()
        {
            var counts = new Dictionary<string, int>();

            var collector = new FilteredElementCollector(_uIDocument.Document)
                .WhereElementIsNotElementType()
                .OfClass(typeof(FamilyInstance))
                .Cast<FamilyInstance>();

            foreach (var instance in collector)
            {
                string familyName = instance.Symbol.Family.Name;
                counts[familyName] = counts.ContainsKey(familyName) ? counts[familyName] + 1 : 1;
            }

            return counts;
        }
    }
}
