﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daramkun.DaramRenamer.Processors.Date
{
	public class AddDateProcessor : IProcessor
	{
		public string Name { get { return "process_add_date"; } }
		public bool CannotMultithreadProcess { get { return false; } }

		[Globalized ( "add_date_type", 0 )]
		public DateType Type { get; set; } = DateType.CreationDate;
		[Globalized ( "add_date_format", 1 )]
		public string Format { get; set; } = "yyMMdd";
		[Globalized ( "add_date_pos", 2 )]
		public OnePointPosition Position { get; set; } = OnePointPosition.EndPoint;

		public bool Process ( FileInfo file )
		{
			string fn = Path.GetFileNameWithoutExtension ( file.ChangedFilename );
			string ext = Path.GetExtension ( file.ChangedFilename );
			string date;
			switch ( Type)
			{
				case DateType.CreationDate: date = File.GetCreationTime ( file.OriginalFullPath ).ToString ( Format ); break;
				case DateType.ModifiedDate: date = File.GetLastWriteTime ( file.OriginalFullPath ).ToString ( Format ); break;
				case DateType.AccessedDate: date = File.GetLastAccessTime ( file.OriginalFullPath ).ToString ( Format ); break;
				case DateType.Now: date = DateTime.Now.ToString ( Format ); break;
				default: return false;
			}
			file.ChangedFilename = Position == OnePointPosition.StartPoint ? $"{date}{fn}{ext}" :
				( Position == OnePointPosition.EndPoint ? $"{fn}{date}{ext}" : $"{date}{fn}{date}{ext}" );
			return true;
		}
	}
}
