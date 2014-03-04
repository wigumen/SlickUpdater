using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;

namespace SlickUpdater {
	static class DragAndDrop {

		public static Point startPoint;

		public static void outputDirListBox_PreviewMouseMove(object sender, MouseEventArgs e) {
			Point mousePos = e.GetPosition(null);
			Vector diff = startPoint - mousePos;

			if (e.LeftButton == MouseButtonState.Pressed && (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)) {

				DataGrid dataGrid = sender as DataGrid;
				object item = dataGrid.CurrentItem;

				if (item != null) {
					modSourceFolder data = (modSourceFolder)item;

					DataObject dragData = new DataObject("myFormat", data);
					DragDrop.DoDragDrop(dataGrid, dragData, DragDropEffects.Move);
				}
			}
		}

		public static void inputDirListBox_PreviewMouseMove(object sender, MouseEventArgs e) {
			Point mousePos = e.GetPosition(null);
			Vector diff = startPoint - mousePos;

			//Little to no fucking idea how this works... need to do some research...
			if (e.LeftButton == MouseButtonState.Pressed && (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)) {
				// Get the Dragged ListViewItem
				DataGrid dataGrid = sender as DataGrid;
				object item = dataGrid.CurrentItem;
				if (item != null) {
					// Find the data behind the ListViewItem

					modSourceFolder data = (modSourceFolder)item;

					//Initialize the drag & drop operation
					DataObject dragData = new DataObject("myFormat", data);

					DragDrop.DoDragDrop(dataGrid, dragData, DragDropEffects.Move);
				}
			}
		}

		// Helper to search up the VisualTree
		private static T FindAnchestor<T>(DependencyObject current)
			where T : DependencyObject {
			do {
				if (current is T) {
					return (T)current;
				}
				current = VisualTreeHelper.GetParent(current);
			}
			while (current != null);
			return null;
		}


		public static void inputDirListBox_DragEnter(object sender, DragEventArgs e) {
			if (!e.Data.GetDataPresent("myFormat") || sender == e.Source) {
				e.Effects = DragDropEffects.None;
			}
		}

		public static void outputDirListBox_DragEnter(object sender, DragEventArgs e) {
			if (!e.Data.GetDataPresent("myFormat") || sender == e.Source) {
				e.Effects = DragDropEffects.None;
			}
		}

		public static void inputDirListBox_Drop(object sender, DragEventArgs e) {
			if (e.Data.GetDataPresent("myFormat")) {
				modSourceFolder data = e.Data.GetData("myFormat") as modSourceFolder;
				DataGrid dataGrid = sender as DataGrid;

				bool exists = false;
				for (int i = 0; i < dataGrid.Items.Count; i++) {
					
					if ((dataGrid.Items.GetItemAt(i) as modSourceFolder).modFolderName == data.modFolderName) {
						exists = true;
					}
				}
				if (!exists) {
					DataGrid outputDir = WindowManager.mainWindow.outputDirListBox;


					/*
					for (int i = 0; i < outputDir.Items.Count; i++ ) {
						if ((outputDir.Items.GetItemAt(i) as modSourceFolder).modFolderName == data.modFolderName) {
							outputDir.Items.Remove()
						}
					}
					 */
					List<modSourceFolder> outputSource = WindowManager.mainWindow.outputDirListBox.ItemsSource as List<modSourceFolder>;
					outputSource.Remove(data);
					List<modSourceFolder> source = dataGrid.ItemsSource as List<modSourceFolder>;
					source.Add(data);
					dataGrid.Items.Refresh();
					WindowManager.mainWindow.outputDirListBox.Items.Refresh();
				}
			}
		}

		public static void outputDirListBox_Drop(object sender, DragEventArgs e) {
			if (e.Data.GetDataPresent("myFormat")) {
				modSourceFolder data = e.Data.GetData("myFormat") as modSourceFolder;
				DataGrid dataGrid = sender as DataGrid;

				bool exists = false;

				for (int i = 0; i < dataGrid.Items.Count; i++)
				{
					if ((dataGrid.Items.GetItemAt(i) as modSourceFolder).modFolderName == data.modFolderName) {
						exists = true;
					}
				}
				if (!exists) {
					DataGrid inputDir = WindowManager.mainWindow.inputDirListBox;
					List<modSourceFolder> modSource = inputDir.ItemsSource as List<modSourceFolder>;
					List<modSourceFolder> outputSource = dataGrid.ItemsSource as List<modSourceFolder>;
					outputSource.Add(data);
					modSource.Remove(data);
					dataGrid.ItemsSource = outputSource;
					dataGrid.Items.Refresh();
					inputDir.Items.Refresh();
				}
			}
		}
	}
}
