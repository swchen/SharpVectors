﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Printing;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Runtime.InteropServices;

namespace WpfW3cSvgTestSuite
{
    /// <summary>
    /// Interaction logic for SvgTestResultsWindow.xaml
    /// </summary>
    public partial class SvgTestResultsWindow : Window
    {
        private IList<string> _categoryLabels;

        private IList<SvgTestResult> _testResults;

        public SvgTestResultsWindow()
        {
            InitializeComponent();

            this.Loaded += OnWindowLoaded;
        }

        public IList<SvgTestResult> Results
        {
            get {
                return _testResults;
            }
            set {
                _testResults = value;
            }
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            double width = SystemParameters.PrimaryScreenWidth;
            double height = SystemParameters.PrimaryScreenHeight;

            //this.Width = Math.Min(1200, width) * 0.90;
            this.Height = Math.Min(1080, height) * 0.90;

            //this.Left = (width - this.Width) / 2.0;
            //this.Top = (height - this.Height) / 2.0;

            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            HideMinimizeAndMaximizeButtons(this);
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            var pageSize = new PageMediaSize(PageMediaSizeName.ISOA4);

            if (pageSize.Width != null)
            {
                testDetailsDoc.ColumnWidth = pageSize.Width.Value;
            }

            this.CreateDocument();
        }

        private void CreateDocument()
        {
            if (_testResults == null || _testResults.Count == 0)
            {
                return;
            }

            _categoryLabels = new List<string>();

            testDetailsDoc.Blocks.Clear();

            int resultCount = _testResults.Count;
            for (int i = 0; i < resultCount; i++)
            {
                SvgTestResult testResult = _testResults[i];
                if (!testResult.IsValid)
                {
                    continue;
                }
                IList<SvgTestCategory> testCategories = testResult.Categories;
                if (testCategories == null || testCategories.Count == 0)
                {
                    continue;
                }

                if (i == 0)
                {
                    for (int j = 0; j < testCategories.Count; j++)
                    {
                        _categoryLabels.Add(testCategories[j].Label);
                    }
                }

                Section titleSection = new Section();
                Paragraph titlePara = new Paragraph();
                titlePara.FontWeight = FontWeights.Bold;
                titlePara.FontSize = 18;
                titlePara.Inlines.Add(new Run("Test Results: SharpVectors Version = " + testResult.Version));
                titleSection.Blocks.Add(titlePara);

                Paragraph datePara = new Paragraph();
                datePara.FontWeight = FontWeights.Bold;
                datePara.FontSize = 16;
                datePara.TextAlignment = TextAlignment.Center;
                datePara.Padding = new Thickness(3);
                datePara.Inlines.Add(new Run("Test Date: " + testResult.Date.ToString()));
                titleSection.Blocks.Add(datePara);

                testDetailsDoc.Blocks.Add(titleSection);

                Section resultSection = new Section();
                Table resultTable = CreateResultTable(testCategories);

                resultSection.Blocks.Add(resultTable);

                if (resultCount > 1)
                {
                    this.CreateHorzLine(resultSection, (i == (resultCount - 1)));
                }
                else
                {
                    this.CreateHorzLine(resultSection);
                }

                testDetailsDoc.Blocks.Add(resultSection);
            }

            if (resultCount > 1)
            {
                Section summarySection = new Section();
                Paragraph summaryPara = new Paragraph();
                summaryPara.FontWeight = FontWeights.Bold;
                summaryPara.Margin = new Thickness(3, 3, 3, 10);
                summaryPara.FontSize = 18;
                summaryPara.Inlines.Add(new Run("Test Results: Summary"));
                summarySection.Blocks.Add(summaryPara);

                Table summaryTable = CreateSummaryTable();

                summarySection.Blocks.Add(summaryTable);

                this.CreateHorzLine(summarySection, true);

                testDetailsDoc.Blocks.Add(summarySection);
            }

            Section endSection = new Section();
            endSection.Blocks.Add(CreateAlert("Note: Test Suite", "These tests are based on SVG 1.1 First Edition Test Suite: 13 December 2006"));
            Paragraph endPara = new Paragraph();
            endPara.Inlines.Add(new LineBreak());
            endSection.Blocks.Add(endPara);

            testDetailsDoc.Blocks.Add(endSection);
        }

        private void CreateHorzLine(Section section, bool thicker = false)
        {
            Paragraph linePara = new Paragraph();
            linePara.Margin = new Thickness(3, 10, 3, 10);

            int factor = thicker ? 2 : 1;

            var horzLine = new Line {
                X1 = 0,
                Y1 = 0,
                X2 = 1000,
                Y2 = 0,
                Stroke = Brushes.DimGray,
                StrokeThickness = 2 * factor
            };

            linePara.Inlines.Add(horzLine);

            section.Blocks.Add(linePara);
        }

        private Table CreateResultTable(IList<SvgTestCategory> testCategories)
        {
            Table resultTable = new Table();
            resultTable.CellSpacing = 0;
            resultTable.BorderBrush = Brushes.Gray;
            resultTable.BorderThickness = new Thickness(1);
            resultTable.Margin = new Thickness(16, 0, 16, 16);

            TableColumn categoryCol = new TableColumn();
            TableColumn failureCol  = new TableColumn();
            TableColumn successCol  = new TableColumn();
            TableColumn partialCol  = new TableColumn();
            TableColumn unknownCol  = new TableColumn();
            TableColumn totalCol    = new TableColumn();

            categoryCol.Width = new GridLength(2, GridUnitType.Star);
            failureCol.Width  = new GridLength(1, GridUnitType.Star);
            successCol.Width  = new GridLength(1, GridUnitType.Star);
            partialCol.Width  = new GridLength(1, GridUnitType.Star);
            unknownCol.Width  = new GridLength(1, GridUnitType.Star);
            totalCol.Width    = new GridLength(1, GridUnitType.Star);

            resultTable.Columns.Add(categoryCol);
            resultTable.Columns.Add(failureCol);
            resultTable.Columns.Add(successCol);
            resultTable.Columns.Add(partialCol);
            resultTable.Columns.Add(unknownCol);
            resultTable.Columns.Add(totalCol);

            TableRowGroup headerGroup = new TableRowGroup();
            headerGroup.Background = Brushes.LightGray;
            TableRow headerRow = new TableRow();

            headerRow.Cells.Add(CreateHeaderCell("Category", false, false));
            headerRow.Cells.Add(CreateHeaderCell("Failure", false, false));
            headerRow.Cells.Add(CreateHeaderCell("Success", false, false));
            headerRow.Cells.Add(CreateHeaderCell("Partial", false, false));
            headerRow.Cells.Add(CreateHeaderCell("Unknown", false, false));
            headerRow.Cells.Add(CreateHeaderCell("Total", true, false));

            headerGroup.Rows.Add(headerRow);
            resultTable.RowGroups.Add(headerGroup);

            for (int i = 0; i < testCategories.Count; i++)
            {
                SvgTestCategory testCategory = testCategories[i];
                if (!testCategory.IsValid)
                {
                    continue;
                }

                TableRowGroup resultGroup = new TableRowGroup();
                TableRow resultRow = new TableRow();

                bool lastBottom = (i == (testCategories.Count - 1));

                resultRow.Cells.Add(CreateCell(testCategory.Label, false, lastBottom));
                resultRow.Cells.Add(CreateCell(testCategory.Failures, false, lastBottom));
                resultRow.Cells.Add(CreateCell(testCategory.Successes, false, lastBottom));
                resultRow.Cells.Add(CreateCell(testCategory.Partials, false, lastBottom));
                resultRow.Cells.Add(CreateCell(testCategory.Unknowns, false, lastBottom));
                resultRow.Cells.Add(CreateCell(testCategory.Total, true, lastBottom));

                resultGroup.Rows.Add(resultRow);
                resultTable.RowGroups.Add(resultGroup);
            }

            return resultTable;
        }

        private Table CreateSummaryTable()
        {
            int resultCount = _testResults.Count;

            Table resultTable = new Table();
            resultTable.CellSpacing = 0;
            resultTable.BorderBrush = Brushes.Gray;
            resultTable.BorderThickness = new Thickness(1);
            resultTable.Margin = new Thickness(16, 0, 16, 16);

            TableColumn categoryCol = new TableColumn();
            categoryCol.Width = new GridLength(2, GridUnitType.Star);
            resultTable.Columns.Add(categoryCol);

            TableColumn totalCol = new TableColumn();
            totalCol.Width = new GridLength(1, GridUnitType.Star);
            resultTable.Columns.Add(totalCol);

            for (int i = 0; i < resultCount; i++)
            {
                TableColumn successCol  = new TableColumn();
                successCol.Width  = new GridLength(1, GridUnitType.Star);
                resultTable.Columns.Add(successCol);
            }

            TableRowGroup headerGroup = new TableRowGroup();
            headerGroup.Background = Brushes.LightGray;
            TableRow headerRow = new TableRow();

            headerRow.Cells.Add(CreateHeaderCell("Category", false, false));
            headerRow.Cells.Add(CreateHeaderCell("Total", false, false));

            for (int i = 0; i < resultCount; i++)
            {
                SvgTestResult testResult = _testResults[i];
                headerRow.Cells.Add(CreateHeaderCell(testResult.Version + " (%)", (i == (resultCount - 1)), false));
            }

            headerGroup.Rows.Add(headerRow);
            resultTable.RowGroups.Add(headerGroup);

            for (int k = 0; k < _categoryLabels.Count; k++)
            {
                TableRowGroup resultGroup = new TableRowGroup();
                TableRow resultRow = new TableRow();

                bool lastBottom = (k == (_categoryLabels.Count - 1));

                resultRow.Cells.Add(CreateCell(_categoryLabels[k], false, lastBottom));

                for (int i = 0; i < resultCount; i++)
                {
                    SvgTestResult testResult = _testResults[i];

                    IList<SvgTestCategory> testCategories = testResult.Categories;

                    SvgTestCategory testCategory = testCategories[k];
                    if (!testCategory.IsValid)
                    {
                        continue;
                    }

                    int total = testCategory.Total;

                    if (i == 0)
                    {
                        resultRow.Cells.Add(CreateCell(total, true, lastBottom));
                    }

                    bool lastRight = (i == (resultCount - 1));

                    float percentage = testCategory.Successes * 100.0f / total;

                    resultRow.Cells.Add(CreateCell(percentage.ToString("00.00"), lastRight, lastBottom, false, false));
                }

                resultGroup.Rows.Add(resultRow);
                resultTable.RowGroups.Add(resultGroup);
            }

            return resultTable;
        }

        private Table CreateAlert(string title, string message)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                title = "Note";
            }
            if (string.IsNullOrWhiteSpace(message))
            {
                return null;
            }

            Table alertTable = new Table();
            alertTable.CellSpacing = 0;
            alertTable.BorderBrush = Brushes.DimGray;
            alertTable.BorderThickness = new Thickness(1);
            alertTable.Margin = new Thickness(48, 0, 48, 16);

            TableRowGroup headerGroup = new TableRowGroup();
            headerGroup.Background = Brushes.DimGray;
            headerGroup.Foreground = Brushes.White;
            TableRow headerRow = new TableRow();

            headerRow.Cells.Add(CreateCell(title, 0, 0, true, false, false, true, 16));
            headerGroup.Rows.Add(headerRow);
            alertTable.RowGroups.Add(headerGroup);

            TableRowGroup alertGroup = new TableRowGroup();
            TableRow alertRow = new TableRow();

            alertRow.Cells.Add(CreateCell(message, 0, 0, true, true, false, false));

            alertGroup.Rows.Add(alertRow);
            alertTable.RowGroups.Add(alertGroup);

            return alertTable;
        }

        private TableCell CreateHeaderCell(string text, bool lastRight, bool lastBottom, bool boldText = true)
        {
            TableCell tableCell = new TableCell();
            tableCell.BorderBrush = Brushes.Gray;
            tableCell.BorderThickness = new Thickness(0, 0, lastRight ? 0 : 1, lastBottom ? 0 : 1);

            Paragraph cellPara = new Paragraph();
            if (boldText)
            {
                cellPara.FontWeight = FontWeights.Bold;
            }
            cellPara.Inlines.Add(new Run(text));

            tableCell.Blocks.Add(cellPara);

            return tableCell;
        }

        private TableCell CreateCell(string text, int colSpan, int rowSpan, 
            bool lastRight, bool lastBottom, bool filled, bool boldText, int fontSize = 0)
        {
            TableCell tableCell = new TableCell();
            if (filled)
            {
                tableCell.Background = Brushes.DimGray;
            }
            tableCell.BorderBrush = Brushes.DimGray;
            tableCell.BorderThickness = new Thickness(0, 0, lastRight? 0 : 1, lastBottom? 0 : 1);

            if (colSpan > 0)
            {
                tableCell.ColumnSpan = colSpan;
            }
            if (rowSpan > 0)
            {
                tableCell.RowSpan = rowSpan;
            }

            Paragraph cellPara = new Paragraph();
            cellPara.KeepTogether = true;
            if (boldText)
            {
                cellPara.FontWeight = FontWeights.Bold;
            }
            if (fontSize > 1)
            {
                cellPara.FontSize = fontSize;
            }
            cellPara.Inlines.Add(new Run(text));

            tableCell.Blocks.Add(cellPara);

            return tableCell;
        }

        private TableCell CreateCell(string text, bool lastRight, bool lastBottom, bool boldText = false, bool filled = true)
        {
            TableCell tableCell = new TableCell();
            if (filled)
            {
                tableCell.Background = Brushes.LightGray;
            }
            tableCell.BorderBrush = Brushes.Gray;
            tableCell.BorderThickness = new Thickness(0, 0, lastRight? 0 : 1, lastBottom? 0 : 1);

            Paragraph cellPara = new Paragraph();
            cellPara.KeepTogether = true;
            if (boldText)
            {
                cellPara.FontWeight = FontWeights.Bold;
            }
            cellPara.Inlines.Add(new Run(text));

            tableCell.Blocks.Add(cellPara);

            return tableCell;
        }

        private TableCell CreateCell(int number, bool lastRight, bool lastBottom, bool boldText = false)
        {
            TableCell tableCell = new TableCell();
            tableCell.BorderBrush = Brushes.Gray;
            tableCell.BorderThickness = new Thickness(0, 0, lastRight ? 0 : 1, lastBottom ? 0 : 1);

            Paragraph cellPara = new Paragraph();
            cellPara.Inlines.Add(new Run(number.ToString()));
            if (boldText)
            {
                cellPara.FontWeight = FontWeights.Bold;
            }

            tableCell.Blocks.Add(cellPara);

            return tableCell;
        }

        #region InteropServices

        // from winuser.h
        private const int GWL_STYLE = -16;
        private const int WS_MAXIMIZEBOX = 0x10000;
        private const int WS_MINIMIZEBOX = 0x20000;

        [DllImport("user32.dll")]
        extern private static int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        extern private static int SetWindowLong(IntPtr hwnd, int index, int value);

        private static void HideMinimizeAndMaximizeButtons(Window window)
        {
            IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(window).Handle;
            var currentStyle = GetWindowLong(hwnd, GWL_STYLE);

            SetWindowLong(hwnd, GWL_STYLE, (currentStyle & ~WS_MAXIMIZEBOX & ~WS_MINIMIZEBOX));
        }

        #endregion
    }
}
