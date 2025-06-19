
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RZData.Models;
using RZData.Services;
using RZData.Tools;
using RZData.ViewModels;
using System;
using System.Collections.Generic;

namespace UnitTestProject1
{
    [TestClass]
    public class FilterToolTests
    {
        // 测试数据创建辅助方法
        private RevitSolidElement CreateTestElement(string familyCategory, string familyName, string elementName,
            List<ParameterVM> parameters = null, bool isNameCorrect = true, bool isPropertiesCorrect = true)
        {
            return new RevitSolidElement()
            {
                FamilyCategory = familyCategory,
                FamilyName = familyName,
                ElementName = elementName,
                IsNameCorrect = isNameCorrect,
                IsPropertiesCorrect = isPropertiesCorrect,
                Parameters = parameters ?? new List<ParameterVM>()
            };
        }

        #region 等于逻辑测试
        [TestMethod]
        public void FilterRevitElement_Equals_Match()
        {
            // 准备测试数据
            var element = CreateTestElement("墙", "砖墙", "外墙", new List<ParameterVM>
            {
                new ParameterVM { Name = "类型", Value = "承重墙" },
                new ParameterVM { Name = "高度", Value = "3000" }
            });

            var filter = new FilterConditionViewModel
            {
                PropertyName = "类型",
                PropertyValue = "承重墙",
                Logic = "等于"
            };

            // 执行测试
            bool result = FilterTool.FilterRevitElement(element, filter);

            // 验证结果
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void FilterRevitElement_Equals_NoMatch()
        {
            var element = CreateTestElement("墙", "砖墙", "外墙", new List<ParameterVM>
            {
                new ParameterVM { Name = "类型", Value = "承重墙" },
                new ParameterVM { Name = "高度", Value = "3000" }
            });

            var filter = new FilterConditionViewModel
            {
                PropertyName = "类型",
                PropertyValue = "非承重墙",
                Logic = "等于"
            };

            bool result = FilterTool.FilterRevitElement(element, filter);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void FilterRevitElement_Equals_CaseInsensitiveMatch()
        {
            var element = CreateTestElement("墙", "砖墙", "外墙", new List<ParameterVM>
            {
                new ParameterVM { Name = "类型", Value = "CONCRETE WALL" },
                new ParameterVM { Name = "高度", Value = "3000" }
            });

            var filter = new FilterConditionViewModel
            {
                PropertyName = "类型",
                PropertyValue = "concrete wall",
                Logic = "等于"
            };

            bool result = FilterTool.FilterRevitElement(element, filter);
            Assert.IsTrue(result);
        }
        #endregion

        #region 不等于逻辑测试
        [TestMethod]
        public void FilterRevitElement_NotEquals_Match()
        {
            var element = CreateTestElement("墙", "砖墙", "外墙", new List<ParameterVM>
            {
                new ParameterVM { Name = "类型", Value = "承重墙" },
                new ParameterVM { Name = "高度", Value = "3000" }
            });

            var filter = new FilterConditionViewModel
            {
                PropertyName = "类型",
                PropertyValue = "非承重墙",
                Logic = "不等于"
            };

            bool result = FilterTool.FilterRevitElement(element, filter);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void FilterRevitElement_NotEquals_NoMatch()
        {
            var element = CreateTestElement("墙", "砖墙", "外墙", new List<ParameterVM>
            {
                new ParameterVM { Name = "类型", Value = "承重墙" },
                new ParameterVM { Name = "高度", Value = "3000" }
            });

            var filter = new FilterConditionViewModel
            {
                PropertyName = "类型",
                PropertyValue = "承重墙",
                Logic = "不等于"
            };

            bool result = FilterTool.FilterRevitElement(element, filter);
            Assert.IsFalse(result);
        }
        #endregion

        #region 大于等于逻辑测试
        [TestMethod]
        public void FilterRevitElement_GreaterOrEqual_Match()
        {
            var element = CreateTestElement("墙", "砖墙", "外墙", new List<ParameterVM>
            {
                new ParameterVM { Name = "高度", Value = "3000" }
            });

            var filter = new FilterConditionViewModel
            {
                PropertyName = "高度",
                PropertyValue = "2500",
                Logic = "大于等于"
            };

            bool result = FilterTool.FilterRevitElement(element, filter);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void FilterRevitElement_GreaterOrEqual_Equal()
        {
            var element = CreateTestElement("墙", "砖墙", "外墙", new List<ParameterVM>
            {
                new ParameterVM { Name = "高度", Value = "3000" }
            });

            var filter = new FilterConditionViewModel
            {
                PropertyName = "高度",
                PropertyValue = "3000",
                Logic = "大于等于"
            };

            bool result = FilterTool.FilterRevitElement(element, filter);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void FilterRevitElement_GreaterOrEqual_NoMatch()
        {
            var element = CreateTestElement("墙", "砖墙", "外墙", new List<ParameterVM>
            {
                new ParameterVM { Name = "高度", Value = "3000" }
            });

            var filter = new FilterConditionViewModel
            {
                PropertyName = "高度",
                PropertyValue = "3500",
                Logic = "大于等于"
            };

            bool result = FilterTool.FilterRevitElement(element, filter);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void FilterRevitElement_GreaterOrEqual_InvalidParamValue()
        {
            var element = CreateTestElement("墙", "砖墙", "外墙", new List<ParameterVM>
            {
                new ParameterVM { Name = "高度", Value = "three thousand" }
            });

            var filter = new FilterConditionViewModel
            {
                PropertyName = "高度",
                PropertyValue = "3000",
                Logic = "大于等于"
            };

            Assert.ThrowsException<ArgumentException>(() =>
                FilterTool.FilterRevitElement(element, filter));
        }

        [TestMethod]
        public void FilterRevitElement_GreaterOrEqual_InvalidFilterValue()
        {
            var element = CreateTestElement("墙", "砖墙", "外墙", new List<ParameterVM>
            {
                new ParameterVM { Name = "高度", Value = "3000" }
            });

            var filter = new FilterConditionViewModel
            {
                PropertyName = "高度",
                PropertyValue = "three thousand",
                Logic = "大于等于"
            };

            Assert.ThrowsException<ArgumentException>(() =>
                FilterTool.FilterRevitElement(element, filter));
        }
        #endregion

        #region 小于等于逻辑测试
        [TestMethod]
        public void FilterRevitElement_LessOrEqual_Match()
        {
            var element = CreateTestElement("墙", "砖墙", "外墙", new List<ParameterVM>
            {
                new ParameterVM { Name = "高度", Value = "3000" }
            });

            var filter = new FilterConditionViewModel
            {
                PropertyName = "高度",
                PropertyValue = "3500",
                Logic = "小于等于"
            };

            bool result = FilterTool.FilterRevitElement(element, filter);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void FilterRevitElement_LessOrEqual_Equal()
        {
            var element = CreateTestElement("墙", "砖墙", "外墙", new List<ParameterVM>
            {
                new ParameterVM { Name = "高度", Value = "3000" }
            });

            var filter = new FilterConditionViewModel
            {
                PropertyName = "高度",
                PropertyValue = "3000",
                Logic = "小于等于"
            };

            bool result = FilterTool.FilterRevitElement(element, filter);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void FilterRevitElement_LessOrEqual_NoMatch()
        {
            var element = CreateTestElement("墙", "砖墙", "外墙", new List<ParameterVM>
            {
                new ParameterVM { Name = "高度", Value = "3000" }
            });

            var filter = new FilterConditionViewModel
            {
                PropertyName = "高度",
                PropertyValue = "2500",
                Logic = "小于等于"
            };

            bool result = FilterTool.FilterRevitElement(element, filter);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void FilterRevitElement_LessOrEqual_InvalidParamValue()
        {
            var element = CreateTestElement("墙", "砖墙", "外墙", new List<ParameterVM>
            {
                new ParameterVM { Name = "高度", Value = "three thousand" }
            });

            var filter = new FilterConditionViewModel
            {
                PropertyName = "高度",
                PropertyValue = "3000",
                Logic = "小于等于"
            };

            Assert.ThrowsException<ArgumentException>(() =>
                FilterTool.FilterRevitElement(element, filter));
        }

        [TestMethod]
        public void FilterRevitElement_LessOrEqual_InvalidFilterValue()
        {
            var element = CreateTestElement("墙", "砖墙", "外墙", new List<ParameterVM>
            {
                new ParameterVM { Name = "高度", Value = "3000" }
            });

            var filter = new FilterConditionViewModel
            {
                PropertyName = "高度",
                PropertyValue = "three thousand",
                Logic = "小于等于"
            };

            Assert.ThrowsException<ArgumentException>(() =>
                FilterTool.FilterRevitElement(element, filter));
        }
        #endregion

        #region 正则表达式逻辑测试
        [TestMethod]
        public void FilterRevitElement_Regex_Match()
        {
            var element = CreateTestElement("墙", "砖墙", "外墙", new List<ParameterVM>
            {
                new ParameterVM { Name = "编号", Value = "WALL-001" }
            });

            var filter = new FilterConditionViewModel
            {
                PropertyName = "编号",
                PropertyValue = @"^WALL-\d+$",
                Logic = "正则表达式"
            };

            bool result = FilterTool.FilterRevitElement(element, filter);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void FilterRevitElement_Regex_NoMatch()
        {
            var element = CreateTestElement("墙", "砖墙", "外墙", new List<ParameterVM>
            {
                new ParameterVM { Name = "编号", Value = "WALL-A01" }
            });

            var filter = new FilterConditionViewModel
            {
                PropertyName = "编号",
                PropertyValue = @"^WALL-\d+$",
                Logic = "正则表达式"
            };

            bool result = FilterTool.FilterRevitElement(element, filter);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void FilterRevitElement_Regex_InvalidPattern()
        {
            var element = CreateTestElement("墙", "砖墙", "外墙", new List<ParameterVM>
            {
                new ParameterVM { Name = "编号", Value = "WALL-001" }
            });

            var filter = new FilterConditionViewModel
            {
                PropertyName = "编号",
                PropertyValue = @"[a-z", // 无效的正则表达式模式
                Logic = "正则表达式"
            };

            Assert.ThrowsException<ArgumentException>(() =>
                FilterTool.FilterRevitElement(element, filter));
        }
        #endregion

        #region 边界条件测试
        [TestMethod]
        public void FilterRevitElement_ParameterNotFound()
        {
            var element = CreateTestElement("墙", "砖墙", "外墙", new List<ParameterVM>
            {
                new ParameterVM { Name = "类型", Value = "承重墙" }
            });

            var filter = new FilterConditionViewModel
            {
                PropertyName = "高度",
                PropertyValue = "3000",
                Logic = "等于"
            };

            bool result = FilterTool.FilterRevitElement(element, filter);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void FilterRevitElement_ParameterValueEmpty()
        {
            var element = CreateTestElement("墙", "砖墙", "外墙", new List<ParameterVM>
            {
                new ParameterVM { Name = "高度", Value = "" }
            });

            var filter = new FilterConditionViewModel
            {
                PropertyName = "高度",
                PropertyValue = "3000",
                Logic = "等于"
            };

            bool result = FilterTool.FilterRevitElement(element, filter);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void FilterRevitElement_NullParameters()
        {
            var element = CreateTestElement("墙", "砖墙", "外墙");
            element.Parameters = null;

            var filter = new FilterConditionViewModel
            {
                PropertyName = "高度",
                PropertyValue = "3000",
                Logic = "等于"
            };

            bool result = FilterTool.FilterRevitElement(element, filter);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void FilterRevitElement_EmptyParameters()
        {
            var element = CreateTestElement("墙", "砖墙", "外墙", new List<ParameterVM>());

            var filter = new FilterConditionViewModel
            {
                PropertyName = "高度",
                PropertyValue = "3000",
                Logic = "等于"
            };

            bool result = FilterTool.FilterRevitElement(element, filter);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void FilterRevitElement_UnsupportedLogic()
        {
            var element = CreateTestElement("墙", "砖墙", "外墙", new List<ParameterVM>
            {
                new ParameterVM { Name = "高度", Value = "3000" }
            });

            var filter = new FilterConditionViewModel
            {
                PropertyName = "高度",
                PropertyValue = "3000",
                Logic = "包含" // 不支持的逻辑
            };

            Assert.ThrowsException<ArgumentException>(() =>
                FilterTool.FilterRevitElement(element, filter));
        }
        #endregion
    }
}
