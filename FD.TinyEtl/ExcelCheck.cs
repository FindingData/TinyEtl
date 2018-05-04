///////////////////////////////////////////////////////////
//  ExcelCheck.cs
//  Implementation of the Class ExcelCheck
//  Generated by Enterprise Architect
//  Created on:      19-4月-2018 12:00:17
//  Original author: drago
///////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;


using System.Data;
using Rules;
using NPOI.SS.UserModel;
using Excels;
using NPOI.Util;

public class ExcelCheck {

    private const string NullHeader = "NullHeader";

    private ExcelCheckReport _checkReport = new ExcelCheckReport();

    private IWorkbook _workbook;
    private ExcelStructure _excelStructure;
    private IList<TemplateRule> _templateRules;

    private DataSet _dataSet { get; set; }

    public TemplateRule _templateRule { get; set; }

    //public IList<ExcelCheckError> ErrorList { get; set; }

    //匹配


    #region 类型格式化

    /// <summary>
    /// integer,number
    /// </summary>
    readonly string[] _fromNumeric = { "integer", "number" };

    /// <summary>
    /// integer
    /// </summary>
    readonly string[] _fromInt = { "integer" };

    /// <summary>
    /// number
    /// </summary>
    readonly string[] _fromDouble = { "number" };

    /// <summary>
    /// date,datetime
    /// </summary>
    readonly string[] _fromData = { "date" }; //{ "date", "datetime" };

    /// <summary>
    /// nvarchar2,varchar2
    /// </summary>
    readonly string[] _fromString = { "nvarchar2", "varchar2" };

    /// <summary>
    /// single_dic
    /// </summary>
    readonly string[] _fromSingleDic = { "single_dic" };

    /// <summary>
    /// multi_dic
    /// </summary>
    readonly string[] _fromMultiDic = { "multi_dic" };

    #endregion


    /// 
    /// <param name="stream"></param>
    public ExcelCheck(Stream stream)
    {
        try
        {
            _workbook = WorkbookFactory.Create(stream);
        }
        catch (Exception ex)
        {
            var message = "---读取Excel文件时遇到未知错误，如果确定文件有效并且未被其它软件打开，请联系系统管理员!\r\n" + ex.Message;
            _checkReport.ExcelReadError("文件读取失败", message);
            _workbook = null;
        }
    }

    /// <summary>
    /// 读取Excel文档结构
    /// </summary>
    /// <returns></returns>
    private void InitExcelStructure()
    {
        try
        {
            _excelStructure = new ExcelStructure();
            var sheets = _excelStructure.Sheets;

            var excelTablesName = GetExcelTablesName(); //获取excel表头
            if (excelTablesName == null || !excelTablesName.Any()) //1.0、无工作表
            {
                _checkReport.ExcelStructureError("无工作表", "没有找到工作表，请检查Excel是否有效!");

            }
            else
            {
                var sheetIndex = -1;
                foreach (var sheetName in excelTablesName)
                {
                    sheetIndex++;
                    if (string.IsNullOrWhiteSpace(sheetName))
                        _checkReport.ExcelStructureError("工作表名称无效", $"----第{sheetIndex + 1}个工作表名称无效!");
                    else
                    {
                        var aSheet = new ExcelSheet()
                        {
                            SheetIndex = sheetIndex,
                            SheetName = sheetName
                        };
                        var columns = aSheet.Columns;
                        var sheet = _workbook.GetSheet(sheetName);
                        var headerrow = sheet.GetRow(0);
                        if (headerrow == null)
                        {
                            _checkReport.ExcelStructureError("存在空工作表", $"----第{sheetIndex + 1}个工作表,为空工作表!");
                        }
                        else
                        {
                            var lastCellNum = headerrow.LastCellNum;
                            var continueNullHeader = 0;
                            for (int colIndex = 0; colIndex < lastCellNum; colIndex++)
                            {
                                try
                                {
                                    var cell = headerrow.GetCell(colIndex);
                                    if (cell == null || cell.ToString().Trim() == "") //1.3 存在为空列名称
                                    {
                                        //aCol.ColumnName = NullHeader + colIndex;
                                        //cols.Add(aCol); 空列头不加入到表结构中
                                        continueNullHeader++;
                                        if (continueNullHeader > 2) break;
                                    }
                                    else
                                    {
                                        continueNullHeader = 0;
                                        var value = cell.CellType == CellType.String ?
                                            cell.StringCellValue.Trim() : cell.ToString().Trim();
                                        if (columns.Any(f => f.ColumnName == Convert.ToString(value))) //1.4存在重复列名
                                        {
                                            _checkReport.ExcelStructureError("列名称重复", $"----[{sheetName}]工作表,存在多个名称为[{value}]的列!");
                                        }
                                        else
                                        {
                                            var aCol = new ExcelColumn()
                                            {
                                                ColumnIndex = colIndex,
                                                ColumnName = value,
                                            };
                                            columns.Add(aCol);
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _checkReport.ExcelStructureError("列名称读取失败", $"----[{sheetName}]工作表,第{colIndex + 1},列名称读取失败!");
                                }
                            }
                        }
                        sheets.Add(aSheet);
                    }
                }

            }
        }
        catch (Exception ex)
        {
            _checkReport.ExcelStructureError("读取表结构出现错误", $"----[错误信息]{ex.Message}");
        }
    }

    /// <summary>
    /// 遍历模版列表,匹配模版
    /// </summary>
    /// <returns></returns>
    private void MatchExcelTemplate()
    {
        foreach (var aTemplateRule in _templateRules)
        {
            if (MatchTemplate(aTemplateRule)) //匹配到第一个ok的模版
            {
                _templateRule = aTemplateRule;
                //匹配成功则清除错误
                _checkReport.Clear(CheckErrorType.TemplateMatch);
                break;
            }
        }

    }

    /// <summary>
    /// 匹配Excel模版
    /// </summary>
    /// <param name="aTemplateRule"></param>
    /// <param name="matchErrorList"></param>
    /// <returns></returns>
    private bool MatchTemplate(TemplateRule aTemplateRule)
    {
        if (aTemplateRule.SheetNum == _excelStructure.SheetNum)
        {
            foreach (var aSheet in _excelStructure.Sheets)
            {
                if (aTemplateRule.SheetRules.Any(s => s.SheetName == aSheet.SheetName))
                {
                    var unMatchColumns = aTemplateRule[aSheet.SheetName].ColumnRules
                        .Where(c => !aSheet.Columns.Any(a => a.ColumnName == c.ColumnName))
                        .Select(c => c.ColumnName)
                        .ToList();
                    if (!unMatchColumns.Any())
                        continue;
                    var message = $"----工作表[{aSheet.SheetName}]不包含列:[{ConvertListToString(unMatchColumns, "]、[")}]与模板《{aTemplateRule.TemplateName}》.[{aSheet.SheetName}]，不匹配！";
                    _checkReport.TemplateMatchError("匹配列失败", message);
                    return false;
                }
                else //2.1.2 模版不包含Excel中的某一个Sheet页,或者Sheet页中列数不一致
                {
                    var message = $"----模板《{aTemplateRule.TemplateName}》中没有找到与[{aSheet.SheetName}](共{aSheet.ColumnNum}列)匹配的工作表，不匹配!";
                    _checkReport.TemplateMatchError("匹配工作表失败", message);
                    return false;
                }
            }
        }
        else
        {
            var message = $"----模版<<{aTemplateRule.TemplateName}>>有{aTemplateRule.SheetNum}个工作表;而Excel中有{_excelStructure.SheetNum}个工作表,不匹配!";
            _checkReport.TemplateMatchError("匹配工作表页数失败", message);
            return false;
        }
        return true;
    }


    private void InitDatSet()
    {
        if (_dataSet == null)
            _dataSet = new DataSet();
        foreach (var aSheetRule in _templateRule.SheetRules)
        {
            if (aSheetRule.ColumnNum > 0)
            {
                _dataSet.Tables.Add(InitDatTable(aSheetRule));
            }
        }
    }


    private DataTable InitDatTable(SheetRule aSheetRule)
    {
        var aSheetStructure = _excelStructure[aSheetRule.SheetName];
        var sheet = _workbook.GetSheetAt(aSheetRule.SheetIndex);
        var table = new DataTable(aSheetRule.SheetName);
        foreach (var aColumn in aSheetStructure.Columns)
        {
            table.Columns.Add(aColumn.ColumnName);
        }

        var iRowNo = 0;
        var continueNullRowNum = 0; //如果联系3行为空,则跳过后续数据检查

        var rowEnumerator = sheet.GetRowEnumerator();
        rowEnumerator.MoveNext();//跳过第一行名称行
        while (rowEnumerator.MoveNext())
        {
            iRowNo++;
            var iNullColumnNum = 0;
            var current = (IRow)rowEnumerator.Current;
            var rowNew = table.NewRow();
            foreach (var aColumnStructure in aSheetStructure.Columns)
            {
                if (aColumnStructure.ColumnName.Contains(NullHeader))
                {
                    iNullColumnNum++;
                    continue;
                }
                var aColumnRule = aSheetRule[aColumnStructure.ColumnName];
                if (aColumnRule == null)
                {
                    iNullColumnNum++;
                    continue;
                }

                string value = null;
                var cell = current.GetCell(aColumnStructure.ColumnIndex);
                var cellTypeText = string.Empty;
                try
                {
                    if (cell != null && cell.ToString().Trim() != string.Empty)
                    {
                        switch (cell.CellType)
                        {
                            case CellType.String:
                                value = cell.StringCellValue.Trim();
                                break;
                            case CellType.Formula:
                                switch (cell.CachedFormulaResultType)
                                {
                                    case CellType.String:
                                        value = cell.StringCellValue.Trim();
                                        break;
                                    case CellType.Numeric:
                                        if (DateUtil.IsCellDateFormatted(cell))
                                        {
                                            cellTypeText = "日期";
                                            value = cell.DateCellValue.ToString();
                                        }
                                        else
                                        {
                                            cellTypeText = "数字";
                                            value = cell.NumericCellValue.ToString();
                                        }
                                        break;
                                    default:
                                        value = cell.ToString().Trim();
                                        break;
                                }

                                break;
                            case CellType.Numeric:
                                if (DateUtil.IsCellDateFormatted(cell))
                                {
                                    value = cell.DateCellValue.ToString();
                                    break;
                                }
                                // 若是自定义时间格式，仍无法判断
                                value = cell.ToString().Trim();
                                break;
                            default:
                                value = cell.ToString().Trim();
                                break;
                        }
                    }
                }
                catch (Exception ex) //3.1数据类型检测
                {
                    var message = $"----[读值失败]：尝试读取第{iRowNo}行 【{aColumnStructure.ColumnName}】的值失败，请手动将该单元格格式为“{cellTypeText}”!";
                    _checkReport.ColumnError($"[{aSheetStructure.SheetName}]表.[{aColumnStructure.ColumnName}]列", message);
                }
                if (value == null)
                {
                    iNullColumnNum++;
                }
                rowNew[aColumnStructure.ColumnIndex] = value;
            }
            if (iNullColumnNum >= aSheetStructure.ColumnNum)//全行为空
            {
                iRowNo--;
                continueNullRowNum++;
                if (continueNullRowNum >= 2) //全空超过3行,则跳过读取
                {
                    break;
                }
                continue;
            }
            continueNullRowNum = 0;
            table.Rows.Add(rowNew);
        }
        return table;
    }


    private void CheckDataSet()
    {
        #region ---循环遍历Table
        foreach (DataTable aTable in _dataSet.Tables)
        {
            var aTableRowList = aTable.Select().ToList();
            var aSheetRule = _templateRule[aTable.TableName];
            var aSheetStructure = _excelStructure[aTable.TableName];

            var checkNullColName = new List<string>();

            #region 列检测
            foreach (var aColumnStructure in aSheetStructure.Columns)
            {
                if (aColumnStructure == null || aColumnStructure.ColumnName.Contains(NullHeader))
                    continue;

                var aColumnRule = aSheetRule[aColumnStructure.ColumnName];
                if (aColumnRule == null)
                    continue;
                List<string> columnValues;
                int iRowNum;
                var ErrorKey = $"[{aSheetStructure.SheetName}]表.[{aColumnStructure.ColumnName}]列";
                #region //不可控检查
                if (!aColumnRule.IsNullable)
                {
                    iRowNum = aTable.Select(String.Format("[{0}] is null ", aColumnStructure.ColumnName)).Count();
                    if (iRowNum > 0)
                    {
                        _checkReport.ColumnError(ErrorKey, $"----[{"不可为空检查"}]不通过：该列为必填字段，有{iRowNum}值为空！");
                    }
                }
                #endregion

                #region //"字符串" aColumnRule.MeanDataType =="字符串"
                if (aColumnRule.MeanDataType.ToLower() == "nvarchar2")
                {
                    iRowNum = aTable.Select(String.Format("len([{0}]) > {1} ", aColumnRule.ColumnName, aColumnRule.FieldLength)).Count();
                    if (iRowNum > 0)
                    {
                        _checkReport.ColumnError(ErrorKey, $"----[{"字符串长度检测"}]不通过：该列数据长度应当小于等于{aColumnRule.FieldLength}，共有{iRowNum}值长度超过{aColumnRule.FieldLength}！");
                    }
                }

                if (aColumnRule.MeanDataType.ToLower() == "nvarchar2")
                {
                    iRowNum = aTable.Select(String.Format("len([{0}]) > {1} ", aColumnRule.ColumnName, aColumnRule.FieldLength)).Count();
                    if (iRowNum > 0)
                    {
                        _checkReport.ColumnError(ErrorKey, $"----[{"字符串长度检测"}]不通过：该列数据长度应当小于等于{aColumnRule.FieldLength}，共有{iRowNum}值长度超过{aColumnRule.FieldLength}！");
                    }
                }
                #endregion

                #region //"单字典型"
                if (_fromSingleDic.Contains(aColumnRule.MeanDataType.ToLower()))
                {
                    columnValues = aTable.Select(String.Format("[{0}] is not null ", aColumnStructure.ColumnName)).ToList()
                                    .Select(p => p[aColumnStructure.ColumnIndex].ToString())
                                    .Distinct().ToList();
                    foreach (var aParName in columnValues)
                    {
                        if (aColumnRule.DictList != null && aColumnRule.DictList.ContainsKey(aParName))
                        {
                            foreach (var aRow in aTable.Select(String.Format("[{0}] = '{1}' ", aColumnStructure.ColumnName, aParName)))
                            {
                                aRow[aColumnRule.ColumnName] = aColumnRule.DictList[aParName].ToString();
                            }
                        }
                        else
                        {
                            iRowNum = aTable.Select(String.Format("[{0}] = '{1}' ", aColumnStructure.ColumnName, aParName)).Count();
                            string checkFromDict = string.Empty;
                            if (aColumnRule.DictList != null)
                            {
                                foreach (var aDict in aColumnRule.DictList)
                                {
                                    if (string.IsNullOrEmpty(checkFromDict))
                                        checkFromDict = "----[注]可选填内容为：" + "“" + aDict.Key + "”";
                                    else
                                        checkFromDict = checkFromDict + "," + "“" + aDict.Key + "”";
                                }
                                checkFromDict += ";";
                            }
                            else
                            {
                                checkFromDict = "----系统还没有为本公司配置“" + aColumnRule.ColumnName + "”的可选内容，请联系系统管理员！";
                            }
                            _checkReport.ColumnError(ErrorKey, checkFromDict);

                            _checkReport.ColumnError(ErrorKey, $"----[字典检测]不通过：“{aColumnRule.ColumnName}”对应字典项出现问题，共有{iRowNum}为该值，请联系管理员！");
                        }

                    }

                }
                #endregion

                #region //"多字典型"
                if (_fromMultiDic.Contains(aColumnRule.MeanDataType.ToLower()))
                {
                    columnValues = aTable.Select(String.Format("[{0}] is not null ", aColumnStructure.ColumnName)).ToList()
                                    .Select(p => p[aColumnStructure.ColumnIndex].ToString())
                                    .Distinct().ToList();
                    foreach (var aParNameStr in columnValues)
                    {
                        iRowNum = aTable.Select(String.Format("[{0}] = '{1}' ", aColumnStructure.ColumnName, aParNameStr)).Count();
                        var aParNameValue = string.Empty;
                        aParNameValue = aParNameStr.Replace("，", ",");
                        aParNameValue = aParNameStr.Replace("、", ",");
                        var aParNameList = aParNameStr.Split(',').Distinct().ToList();
                        aParNameList.RemoveAll(f => string.IsNullOrEmpty(f.Trim()));
                        if (aParNameList != null)
                        {
                            foreach (var aParName in aParNameList)
                            {
                                if (aColumnRule.DictList != null && aColumnRule.DictList.ContainsKey(aParName))
                                {
                                    if (aParNameValue == null)
                                        aParNameValue = aColumnRule.DictList[aParName].ToString();
                                    else
                                        aParNameValue = aParNameValue + "," + aColumnRule.DictList[aParName];
                                }
                                else
                                {
                                    iRowNum = aTable.Select(String.Format("[{0}] = '{1}' ", aColumnStructure.ColumnName, aParName)).Count();
                                    string checkFromDict = string.Empty;
                                    if (aColumnRule.DictList != null)
                                    {
                                        foreach (var aDict in aColumnRule.DictList)
                                        {
                                            if (string.IsNullOrEmpty(checkFromDict))
                                                checkFromDict = "----[注]可选填内容为：" + "“" + aDict.Key + "”";
                                            else
                                                checkFromDict = checkFromDict + "," + "“" + aDict.Key + "”";
                                        }
                                        checkFromDict += ";";
                                    }
                                    else
                                    {
                                        checkFromDict = "----系统还没有为本公司配置“" + aColumnRule.ColumnName + "”的可选内容，请联系系统管理员！";
                                    }
                                    _checkReport.ColumnError(ErrorKey, checkFromDict);

                                    _checkReport.ColumnError(ErrorKey, $"----[字典检测]不通过：“{aColumnRule.ColumnName}”对应字典项出现问题，共有{iRowNum}为该值，请联系管理员！");
                                }
                            }
                        }
                        if (aParNameValue != null)
                        {
                            foreach (var aRow in aTable.Select(String.Format("[{0}] = '{1}' ", aColumnStructure.ColumnName, aParNameStr)))
                            {
                                aRow[aColumnRule.ColumnName] = aParNameValue;
                            }
                        }
                    }
                }
                #endregion

                #region //"小数型":"十进制数":默认两位小数
                if (_fromDouble.Contains(aColumnRule.MeanDataType.ToLower()))
                {
                    columnValues = aTable.Select(String.Format("[{0}] is not null ", aColumnStructure.ColumnName)).ToList()
                                                          .Select(p => p[aColumnStructure.ColumnIndex].ToString())
                                                          .Distinct().ToList();
                    foreach (var aValue in columnValues)
                    {
                        decimal? aDecimalValue = null;
                        if (aValue.EndsWith("%"))
                        {
                            var aStrValue = aValue.TrimEnd('%');
                            aDecimalValue = decimal.TryParse(aStrValue, out var tmpVal) ? tmpVal / 100 : (decimal?)null;
                        }
                        else
                        {
                            aDecimalValue = decimal.TryParse(aValue, out var tmpVal) ? tmpVal : (decimal?)null;
                        }

                        if (aDecimalValue.HasValue && aColumnRule.FieldSacle > 0)
                        {
                            aDecimalValue = Math.Round(aDecimalValue.Value, aColumnRule.FieldSacle);
                        }

                        if (!aDecimalValue.HasValue)
                        {
                            iRowNum = aTable.Select(String.Format("[{0}] = '{1}' ", aColumnStructure.ColumnName, aValue)).Count();
                            _checkReport.ColumnError(ErrorKey, $"----[小数型检测]不通过，“{aValue}”不能转化为{aColumnRule.FieldSacle}位小数，共有{iRowNum}处为该值！");
                        }
                        else if (aColumnRule.DecimalMax.HasValue && aDecimalValue < aColumnRule.DecimalMax)
                        {
                            iRowNum = aTable.Select(String.Format("[{0}] = '{1}' ", aColumnStructure.ColumnName, aValue)).Count();
                            _checkReport.ColumnError(ErrorKey, $"----[小数范围检测]不通过，“{aValue}”必须小于{aColumnRule.DecimalMax}，共有{iRowNum}处为该值！");
                        }
                        else if (aColumnRule.DecimalMin.HasValue && aDecimalValue < aColumnRule.DecimalMin)
                        {
                            iRowNum = aTable.Select(String.Format("[{0}] = '{1}' ", aColumnStructure.ColumnName, aValue)).Count();
                            _checkReport.ColumnError(ErrorKey, $"----[小数范围检测]不通过，“{aValue}”必须大于{aColumnRule.DecimalMin}，共有{iRowNum}处为该值！");
                        }
                        else
                        {
                            if (aValue.EndsWith("%")) //设置为小数值
                            {
                                foreach (var aRow in aTable.Select(String.Format("[{0}] = '{1}' ", aColumnStructure.ColumnName, aValue)))
                                {
                                    aRow[aColumnRule.ColumnName] = aDecimalValue.Value;
                                }
                            }
                        }


                    }
                }
                #endregion

                #region //"整数型"
                if (_fromInt.Contains(aColumnRule.MeanDataType.ToLower()))
                {
                    columnValues = aTable.Select(String.Format("[{0}] is not null ", aColumnStructure.ColumnName)).ToList()
                                                          .Select(p => p[aColumnStructure.ColumnIndex].ToString())
                                                          .Distinct().ToList();
                    foreach (var aValue in columnValues)
                    {
                        int? aIntValue = null;
                        aIntValue = Int32.TryParse(aValue, out var tmpVal) ? tmpVal : (int?)null;
                        if (!aIntValue.HasValue)
                        {
                            iRowNum = aTable.Select(String.Format("[{0}] = '{1}' ", aColumnStructure.ColumnName, aValue)).Count();
                            _checkReport.ColumnError(ErrorKey, $"----[整数型检测]不通过，“{aValue}”不能转化为整数，共有{iRowNum}处为该值！");
                        }
                        else if (aColumnRule.DecimalMax.HasValue && aIntValue < aColumnRule.DecimalMax)
                        {
                            iRowNum = aTable.Select(String.Format("[{0}] = '{1}' ", aColumnStructure.ColumnName, aValue)).Count();
                            _checkReport.ColumnError(ErrorKey, $"----[整数范围检测]不通过，“{aValue}”必须小于{aColumnRule.DecimalMax}，共有{iRowNum}处为该值！");
                        }
                        else if (aColumnRule.DecimalMin.HasValue && aIntValue < aColumnRule.DecimalMin)
                        {
                            iRowNum = aTable.Select(String.Format("[{0}] = '{1}' ", aColumnStructure.ColumnName, aValue)).Count();
                            _checkReport.ColumnError(ErrorKey, $"----[整数范围检测]不通过，“{aValue}”必须大于{aColumnRule.DecimalMin}，共有{iRowNum}处为该值！");
                        }



                    }
                }
                #endregion

                #region //"日期型"
                if (aColumnRule.MeanDataType.ToLower()=="date")
                {
                    columnValues = aTable.Select(String.Format("[{0}] is not null ",
                        aColumnStructure.ColumnName)).ToList()
                                   .Select(p => p[aColumnStructure.ColumnIndex].ToString())
                                   .Distinct().ToList();
                    foreach (var aValue in columnValues)
                    {
                        DateTime? aDateTime = null;
                        aDateTime = DateTime.TryParse(aValue, out var tmpVal) ? tmpVal : (DateTime?)null;
                        if (!aDateTime.HasValue)
                        {
                            iRowNum = aTable.Select(String.Format("[{0}] = '{1}' ", aColumnStructure.ColumnName, aValue)).Count();
                            _checkReport.ColumnError(ErrorKey, $"----[日期型检测]不通过，“{aValue}”不能转化为日期，共有{iRowNum}处为该值！");
                        }
                        else if (aColumnRule.DateTimeMax.HasValue && aDateTime>aColumnRule.DateTimeMax)
                        {
                            iRowNum = aTable.Select(String.Format("[{0}] = '{1}' ", aColumnStructure.ColumnName, aValue)).Count();
                            _checkReport.ColumnError(ErrorKey, $"----[日期范围检测]不通过，“{aValue}”必须小于{aColumnRule.DateTimeMax}，共有{iRowNum}处为该值！");
                        }
                        else if (aColumnRule.DateTimeMin.HasValue&& aDateTime<aColumnRule.DateTimeMin)
                        {
                            iRowNum = aTable.Select(String.Format("[{0}] = '{1}' ", aColumnStructure.ColumnName, aValue)).Count();
                            _checkReport.ColumnError(ErrorKey, $"----[日期范围检测]不通过，“{aValue}”必须大于{aColumnRule.DateTimeMin}，共有{iRowNum}处为该值！");
                        }
                    }

                }
                #endregion


            }
            #endregion //列检测

            var pkList = aSheetRule.PKColumnNameList;
            var fkList = aSheetRule.FKColumnNameList;

            var pkValueList = new Dictionary<string, int>();
            var fkValueListSelf = new Dictionary<string, int>();
            var fkValueListFather = new List<string>();

            #region //主键检测
            if (pkList.Any())
            {
                foreach (var aRow in aTableRowList)
                {
                    string aPkValue = string.Empty;
                    foreach (var aPk in pkList)
                    {
                        aPkValue = aPkValue + string.Format("[{0}]=“{1}”;", aPk, aRow[aPk]);
                    }
                    if (!pkValueList.Keys.Contains(aPkValue))
                    {
                        pkValueList.Add(aPkValue, 1);
                    }
                    else
                    {
                        pkValueList[aPkValue] = pkValueList[aPkValue] + 1;
                    }

                    if(!string.IsNullOrEmpty(aSheetRule.FkSheetName) && fkList.Any())
                    {
                        string aFkValue = string.Empty;
                        foreach (var aFk in fkList.Keys)
                        {
                            aFkValue = aFkValue+ string.Format("[{0}]=“{1}”;", fkList[aFk], aRow[aFk]);
                        }
                        if (!fkValueListSelf.Keys.Contains(aFkValue))
                        {
                            fkValueListSelf.Add(aFkValue, 1);
                        }
                        else
                        {
                            fkValueListSelf[aFkValue] = fkValueListSelf[aFkValue] + 1;
                        }
                    }
                }

                foreach (var aPkError in pkValueList.Where(pk=>pk.Value>1)
                    .Select(pk=>pk.Key).ToList())
                {
                    _checkReport.PkError(aSheetStructure.SheetName, $"----[唯一性检测]不通过，{aPkError}对应{pkValueList[aPkError]}条记录！");
                }
            }
            #endregion //主键检测

            #region //外键检测
            if(fkList.Any() && fkValueListSelf.Count > 0)
            {
                var fDataTable = _dataSet.Tables[aSheetRule.FkSheetName];
                foreach (DataRow aRow in fDataTable.Rows)
                {
                    string aFkValue = string.Empty;
                    foreach (var aFk in fkList.Keys)
                    {
                        aFkValue = aFkValue + string.Format("[{0}]=“{1}”;", fkList[aFk], aRow[fkList[aFk]]);
                    }
                    if (!fkValueListFather.Contains(aFkValue))
                    {
                        fkValueListFather.Add(aFkValue);
                        fkValueListSelf.Remove(aFkValue);
                    }
                    if (fkValueListSelf.Any())
                    {
                        foreach (var aFkError in fkValueListSelf.Keys)
                        {
                            _checkReport.FkError(aSheetStructure.SheetName, $"----[外键检测]不通过，{aFkError}在工作表[{aSheetRule.FkSheetName}]中没有找到对应信息，共有{fkValueListSelf[aFkError]}处为此情况！");
                        }                        
                    }
                }
            }
            #endregion
        }
        #endregion
    }

    private void AppendError(Dictionary<string, List<string>> errorList, string key, string message)
    {
        if (!errorList.Keys.Contains(key))
            errorList.Add(key, new List<string>());
        errorList[key].Add(message);
    }

    private string ConvertListToString(List<string> propertyList, string delimiter = "$$$")
    {
        string result = null;
        if (propertyList != null && propertyList.Any())
        {
            foreach (var a in propertyList)
            {
                if (!string.IsNullOrEmpty(a))
                    result = result + a + delimiter;
            }
        }
        if (result != null)
        {
            result = result.Substring(0, result.Length - delimiter.Length);
        }
        return result;
    }

    private List<string> GetExcelTablesName()
    {
        var list = new List<string>();
        foreach (ISheet sheet in _workbook)
        {
            list.Add(sheet.SheetName);
        }
        return list;
    }

}//end ExcelCheck