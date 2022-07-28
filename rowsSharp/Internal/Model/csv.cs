using System.Collections.ObjectModel;
using System.Collections.Generic;
using CsvHelper.Configuration;

namespace rowsSharp.Model
{
    public class CsvRecord
    {
        public string Column0 { get; set; } = string.Empty;
        public string Column1 { get; set; } = string.Empty;
        public string Column2 { get; set; } = string.Empty;
        public string Column3 { get; set; } = string.Empty;
        public string Column4 { get; set; } = string.Empty;
        public string Column5 { get; set; } = string.Empty;
        public string Column6 { get; set; } = string.Empty;
        public string Column7 { get; set; } = string.Empty;
        public string Column8 { get; set; } = string.Empty;
        public string Column9 { get; set; } = string.Empty;
        public string Column10 { get; set; } = string.Empty;
        public string Column11 { get; set; } = string.Empty;
        public string Column12 { get; set; } = string.Empty;
        public string Column13 { get; set; } = string.Empty;
        public string Column14 { get; set; } = string.Empty;
        public string Column15 { get; set; } = string.Empty;
        public string Column16 { get; set; } = string.Empty;
        public string Column17 { get; set; } = string.Empty;
        public string Column18 { get; set; } = string.Empty;
        public string Column19 { get; set; } = string.Empty;
        public string Column20 { get; set; } = string.Empty;
        public string Column21 { get; set; } = string.Empty;
        public string Column22 { get; set; } = string.Empty;
        public string Column23 { get; set; } = string.Empty;
        public string Column24 { get; set; } = string.Empty;
        public string Column25 { get; set; } = string.Empty;
        public string Column26 { get; set; } = string.Empty;
        public string Column27 { get; set; } = string.Empty;
        public string Column28 { get; set; } = string.Empty;
        public string Column29 { get; set; } = string.Empty;
        public string Column30 { get; set; } = string.Empty;
        public string Column31 { get; set; } = string.Empty;
    }

    public class CsvRecordMap : ClassMap<CsvRecord>
    {
        public CsvRecordMap()
        {
            Map(m => m.Column0).Optional().Index(0);
            Map(m => m.Column1).Optional().Index(1);
            Map(m => m.Column2).Optional().Index(2);
            Map(m => m.Column3).Optional().Index(3);
            Map(m => m.Column4).Optional().Index(4);
            Map(m => m.Column5).Optional().Index(5);
            Map(m => m.Column6).Optional().Index(6);
            Map(m => m.Column7).Optional().Index(7);
            Map(m => m.Column8).Optional().Index(8);
            Map(m => m.Column9).Optional().Index(9);
            Map(m => m.Column10).Optional().Index(10);
            Map(m => m.Column11).Optional().Index(11);
            Map(m => m.Column12).Optional().Index(12);
            Map(m => m.Column13).Optional().Index(13);
            Map(m => m.Column14).Optional().Index(14);
            Map(m => m.Column15).Optional().Index(15);
            Map(m => m.Column16).Optional().Index(16);
            Map(m => m.Column17).Optional().Index(17);
            Map(m => m.Column18).Optional().Index(18);
            Map(m => m.Column19).Optional().Index(19);
            Map(m => m.Column20).Optional().Index(20);
            Map(m => m.Column21).Optional().Index(21);
            Map(m => m.Column22).Optional().Index(22);
            Map(m => m.Column23).Optional().Index(23);
            Map(m => m.Column24).Optional().Index(24);
            Map(m => m.Column25).Optional().Index(25);
            Map(m => m.Column26).Optional().Index(26);
            Map(m => m.Column27).Optional().Index(27);
            Map(m => m.Column28).Optional().Index(28);
            Map(m => m.Column29).Optional().Index(29);
            Map(m => m.Column30).Optional().Index(30);
            Map(m => m.Column31).Optional().Index(31);
        }
    }

    public class Csv
    {
        public const int MaxColumns = 32;
        public List<string> Headers = new();
        public ObservableCollection<CsvRecord> Records = new();
    }
}
