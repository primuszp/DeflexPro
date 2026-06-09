using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using DeflexPro.Model.Events;

namespace DeflexPro.Model
{
    public sealed class KuabFileReader : IFwdFileReader
    {
        #region Custom classes

        private class Header
        {
            public string Description { get; set; }
            public string Content { get; set; }

            public Header(string description, string content)
            {
                this.Description = description;
                this.Content = content;
            }
        }

        private class Installation
        {
            public string Description { get; set; }
            public string Content { get; set; }

            public Installation(string description, string content)
            {
                this.Description = description;
                this.Content = content;
            }
        }

        private class Jump
        {
            public string MeasureType  { get; set; }

            public string MeasureUnit { get; set; }

            public Jump(string type, string unit)
            {
                this.MeasureType = type;
                this.MeasureUnit = unit;
            }
        }

        #endregion

        private string fileName = string.Empty;
        private List<Jump> jumps = new List<Jump>();
        private List<Header> headers = new List<Header>();
        private List<Installation> installations = new List<Installation>();

        private double plateRadius = 150d;
        private Kuab fwdMachine = new Kuab();
        private DateTime created = DateTime.Now;
        private List<Drop> drops = new List<Drop>();
        private List<Sensor> sensors = new List<Sensor>();

        public string FormatName => "KUAB";

        public KuabFileReader()
        {
        }

        public bool CanRead(string fileName)
        {
            using var reader = File.OpenText(fileName);
            return reader.ReadLine()?.StartsWith("IKUAB FWD FILE", StringComparison.OrdinalIgnoreCase) == true;
        }

        private bool Import()
        {
            jumps.Clear();
            headers.Clear();
            installations.Clear();
            drops.Clear();
            sensors.Clear();
            fwdMachine = new Kuab();

            using (StreamReader file = File.OpenText(fileName))
            {
                int count = 0;
                string buffer = string.Empty;
                string[] mtype = null;
                string[] munit = null;

                while ((buffer = file.ReadLine()) != null)
                {
                    try
                    {
                        if (((buffer.IndexOf("H") == 0) || (buffer.IndexOf("B") == 0)) && (buffer.Length >= 20))
                        {
                            string description = buffer.Substring(1, 17).Trim();
                            string content = buffer.Substring(20).Trim();

                            if (description != string.Empty)
                                SetHeader(new Header(description, content));
                        }

                        if ((buffer.IndexOf("I") == 0) && (buffer.Length >= 20))
                        {
                            string description = buffer.Substring(1, 17).Trim();
                            string content = buffer.Substring(20).Trim();

                            SetInstallation(new Installation(description, content));
                        }

                        #region A mérési táblázat fejlécének feldolgozása

                        if ((buffer.IndexOf("J") == 0))
                        {
                            string[] split = buffer.Split(new Char[] { 'J', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                            switch (count++)
                            {
                                case 0: mtype = split;
                                    break;
                                case 1: munit = split;
                                    break;
                                default:
                                    break;
                            }

                            if ((mtype != null) && (munit != null))
                            {
                                for (int i = 0; i < 14; i++)
                                {
                                    string type = "Ismeretlen";
                                    string unit = "Ismeretlen";

                                    if (i < mtype.Length) type = mtype[i];
                                    if (i < munit.Length) unit = munit[i];

                                    jumps.Add(new Jump(type, unit));
                                }
                            }
                        }

                        #endregion

                        #region A mérések feldolgozása

                        if (buffer.IndexOf("D") == 0)
                        {
                            Drop drop = new Drop();

                            string[] split = buffer.Split(new Char[] { 'D', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                            drop.Distance = ParseDouble(split[0]);
                            drop.ImpNumber = int.Parse(split[1], CultureInfo.InvariantCulture.NumberFormat);
                            drop.PeakForce = ParseDouble(split[2]);

                            for (int i = 0; i < sensors.Count; i++)
                                drop.Deflections.Add(new Deflection(sensors[i], ParseDouble(split[3 + i])));

                            drop.AirTemperature = ParseDouble(split[10]);
                            drop.AsphaltTemperature = ParseDouble(split[11]);
                            drop.DateTime = created + TimeSpan.Parse(split[13]);

                            this.drops.Add(drop);
                        }

                        #endregion
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("{0} Exception caught.", e);
                        return false;
                    }
                }
                this.fwdMachine.PlateRadius = plateRadius;
                this.fwdMachine.Sensors = sensors;
                this.fwdMachine.Drops = drops;
                return true;
            }
        }

        private void SetHeader(Header current)
        {
            this.headers.Add(current);
        }

        private void SetInstallation(Installation current)
        {
            switch (current.Description)
            {
                case "Date Created":
                    this.created = ParseDateTime(current.Content);
                    break;
                case "Plate Radius":
                    this.plateRadius = ParseDouble(
                        current.Content.Replace("(cm)", string.Empty).Trim()) * 10d;
                    break;
                case "Sensor Number":
                    {
                        string[] buffer = current.Content.Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        for (int i = 0; i < buffer.Length; i++)
                            this.sensors.Add(new Sensor(int.Parse(buffer[i], CultureInfo.InvariantCulture.NumberFormat), 0, 0));
                    }
                    break;
                case "Sensor Distance":
                    {
                        string[] buffer = current.Content.Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        for (int i = 0; i < buffer.Length - 1; i++)
                            this.sensors[i].X = ParseDouble(buffer[i]) * 10d;
                    }
                    break;
                default:
                    break;
            }
            this.installations.Add(current);
        }

        private DateTime ParseDateTime(string str)
        {
            DateTime result = new DateTime();

            if (str == string.Empty)
                return result;

            if (DateTime.TryParseExact(str, "dd-MM-yyyy", null, DateTimeStyles.NoCurrentDateDefault, out result))
                return result;
            else
                result = DateTime.ParseExact(str, "yyyy.MM.dd.", null, DateTimeStyles.NoCurrentDateDefault);
         
            return result;
        }

        private double ParseDouble(string str)
        {
            double result;

            if (str == string.Empty)
                return double.NaN;

            if (double.TryParse(str.Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out result))
                return result;
            
            return double.NaN;
        }

        public Fwd Read(string fileName)
        {
            this.fileName = fileName;
            if (!Import())
                throw new InvalidDataException("A KUAB fájl nem dolgozható fel.");
            return fwdMachine;
        }
    }
}
