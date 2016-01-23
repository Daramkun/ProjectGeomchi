﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows.Media;
using Microsoft.Win32;

namespace Daramkun.DaramRenamer
{
	public enum RenameMode : byte { Move, Copy }

	public class Optionizer
	{
		const string Filename = "DaramRenamer.config.json";
		const string Key = "Daram Renamer";

		public static Optionizer SharedOptionizer { get; private set; }

		static DataContractJsonSerializer serializer = new DataContractJsonSerializer ( typeof ( Dictionary<string, string> ),
			new DataContractJsonSerializerSettings () { UseSimpleDictionaryFormat = true } );

		public RenameMode RenameMode { get; set; } = RenameMode.Move;
		internal int RenameModeInteger { get { return ( int ) RenameMode; } set { RenameMode = ( RenameMode ) value; } }
		private bool hwAccelMode = true;
		public bool HardwareAccelerationMode
		{
			get { return hwAccelMode; }
			set
			{
				hwAccelMode = value;
				if ( hwAccelMode )
					RenderOptions.ProcessRenderMode = RenderMode.Default;
				else
					RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;
			}
		}
		public bool AutomaticFilenameFix { get; set; } = false;
		public bool AutomaticListCleaning { get; set; } = false;
		public bool OptionSaveToRegistry { get; set; } = true;
		public bool Overwrite { get; set; } = false;

		static Optionizer ()
		{
			SharedOptionizer = new Optionizer ();

			if ( File.Exists ( Filename ) )
			{
				SharedOptionizer.OptionSaveToRegistry = false;

				using ( Stream stream = File.Open ( Filename, FileMode.Open ) )
				{
					var file = serializer.ReadObject ( stream ) as Dictionary<string, string>;
					SharedOptionizer.RenameMode = ( RenameMode ) int.Parse ( file [ "rename_mode" ] );
					SharedOptionizer.HardwareAccelerationMode = bool.Parse ( file [ "hw_accel_mode" ] );
					SharedOptionizer.AutomaticFilenameFix = bool.Parse ( file [ "auto_name_fix" ] );
					SharedOptionizer.AutomaticListCleaning = bool.Parse ( file [ "auto_list_clean" ] );
					SharedOptionizer.Overwrite = bool.Parse ( file [ "overwrite" ] );
					SharedOptionizer.OptionSaveToRegistry = false;
				}
			}
			else
			{
				var userKey = Registry.CurrentUser;
				var swKey = userKey.OpenSubKey ( "SOFTWARE" );
				var daramworldKey = swKey.OpenSubKey ( "DARAM WORLD" );
				if ( daramworldKey != null)
				{
					var renamerKey = daramworldKey.OpenSubKey ( Key );
					if ( renamerKey != null )
					{
						SharedOptionizer.RenameMode = ( RenameMode ) ( int ) renamerKey.GetValue ( "rename_mode", RenameMode.Move );
						SharedOptionizer.HardwareAccelerationMode = ( int ) renamerKey.GetValue ( "hw_accel_mode", 1 ) == 1 ? true : false;
						SharedOptionizer.AutomaticFilenameFix = ( int ) renamerKey.GetValue ( "auto_name_fix", 0 ) == 1 ? true : false;
						SharedOptionizer.AutomaticListCleaning = ( int ) renamerKey.GetValue ( "auto_list_clean", 0 ) == 1 ? true : false;
						SharedOptionizer.Overwrite = ( int ) renamerKey.GetValue ( "overwrite", 0 ) == 1 ? true : false;
						SharedOptionizer.OptionSaveToRegistry = true;
					}
				}
			}
		}

		private Optionizer () { }

		public void Save ()
		{
			if ( OptionSaveToRegistry )
			{
				if ( File.Exists ( Filename ) ) File.Delete ( Filename );

				var userKey = Registry.CurrentUser;
				var swKey = userKey.OpenSubKey ( "SOFTWARE", true );
				var daramworldKey = swKey.OpenSubKey ( "DARAM WORLD", true );
				if ( daramworldKey == null ) daramworldKey = swKey.CreateSubKey ( "DARAM WORLD", true );
				var renamerKey = daramworldKey.OpenSubKey ( Key, true );
				if ( renamerKey == null ) renamerKey = daramworldKey.CreateSubKey ( Key, true );

				renamerKey.SetValue ( "rename_mode", ( int ) RenameMode, RegistryValueKind.DWord );
				renamerKey.SetValue ( "hw_accel_mode", HardwareAccelerationMode ? 1 : 0, RegistryValueKind.DWord );
				renamerKey.SetValue ( "auto_name_fix", AutomaticFilenameFix ? 1 : 0, RegistryValueKind.DWord );
				renamerKey.SetValue ( "auto_list_clean", AutomaticListCleaning ? 1 : 0, RegistryValueKind.DWord );
				renamerKey.SetValue ( "overwrite", Overwrite ? 1 : 0, RegistryValueKind.DWord );
			}
			else
			{
				var userKey = Registry.CurrentUser;
				var swKey = userKey.OpenSubKey ( "SOFTWARE", true );
				var daramworldKey = swKey.OpenSubKey ( "DARAM WORLD", true );
				if ( daramworldKey != null )
				{
					var renamerKey = daramworldKey.OpenSubKey ( Key, true );
					if ( renamerKey != null )
						daramworldKey.DeleteSubKey ( Key, true );
				}

				Dictionary<string, string> map = new Dictionary<string, string> ()
				{
					{ "rename_mode", ( ( int ) RenameMode ).ToString () },
					{ "hw_accel_mode", HardwareAccelerationMode.ToString ().ToLower () },
					{ "auto_name_fix", AutomaticFilenameFix.ToString ().ToLower () },
					{ "auto_list_clean", AutomaticFilenameFix.ToString ().ToLower () },
					{ "overwrite", Overwrite.ToString ().ToLower () },
				};

				using ( Stream stream = File.Open ( Filename, FileMode.Create ) )
					serializer.WriteObject ( stream, map );
			}
		}
	}
}
