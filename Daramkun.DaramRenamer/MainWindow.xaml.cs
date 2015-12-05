﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace Daramkun.DaramRenamer
{
	/// <summary>
	/// MainWindow.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class MainWindow : Window
	{
		ObservableCollection<FileInfo> current = new ObservableCollection<FileInfo> ();
		UndoManager<ObservableCollection<FileInfo>> undoManager = new UndoManager<ObservableCollection<FileInfo>> ();

		public MainWindow ()
		{
			InitializeComponent ();

			Version currentVersion = Assembly.GetEntryAssembly ().GetName ().Version;
			Title = string.Format ( "{3} - v{0}.{1}{2}0", currentVersion.Major, currentVersion.Minor, currentVersion.Build,
				Globalizer.Strings [ "daram_renamer" ] );

			listViewFiles.ItemsSource = current;
        }

		public void AddItem ( string s )
		{
			if ( System.IO.File.Exists ( s ) )
				current.Add ( new FileInfo ( s ) );
			else
				foreach ( string ss in System.IO.Directory.GetFiles ( s ) )
					AddItem ( ss );
		}

		private void listViewFiles_DragEnter ( object sender, DragEventArgs e )
		{
			if ( e.Data.GetDataPresent ( DataFormats.FileDrop ) ) e.Effects = DragDropEffects.None;
		}

		private void listViewFiles_Drop ( object sender, DragEventArgs e )
		{
			if ( e.Data.GetDataPresent ( DataFormats.FileDrop ) )
			{
				undoManager.SaveToUndoStack ( current );

				var temp = e.Data.GetData ( DataFormats.FileDrop ) as string [];
				foreach ( string s in from b in temp orderby b select b ) AddItem ( s );
			}
		}

		private void listViewFiles_KeyUp ( object sender, KeyEventArgs e )
		{
			if ( e.Key == Key.Delete )
			{
				List<FileInfo> tempFileInfos = new List<FileInfo> ();
				foreach ( FileInfo fileInfo in listViewFiles.SelectedItems ) tempFileInfos.Add ( fileInfo );
				foreach ( FileInfo fileInfo in tempFileInfos ) current.Remove ( fileInfo );
				if ( current.Count == 0 ) { undoManager.ClearAll (); }
			}
		}
	}
}
