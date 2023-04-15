using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FilterAndSort {
	public partial class MainWindow : Window {
		private WorkEntities connection;

		// Binding collections properties
		public ObservableCollection<Department> Departments { get; set; }
		public ObservableCollection<Position> Positions { get; set; }
		public ObservableCollection<Employee> Employees { get; set; }

		public ObservableCollection<SortingItem> Sortings { get; set; } = new ObservableCollection<SortingItem> {
			new SortingItem() {DisplayText = "Без сортировки"},
			new SortingItem() {DisplayText = "По ФИО А-Я", Description = new SortDescription("FullName", ListSortDirection.Ascending)},
			new SortingItem() {DisplayText = "По ФИО Я-А", Description = new SortDescription("FullName", ListSortDirection.Descending)},
		};

		private Employee selectedEmployee = null;
		private Department selectedDepartment = null;
		private Position selectedPosition = null;
		private SortingItem selectedSorting = null;
		private string searchText = "";

		public MainWindow() {
			InitializeComponent();

			connection = new WorkEntities();

			Departments = new ObservableCollection<Department>(connection.Departments.ToList());
			Positions = new ObservableCollection<Position>(connection.Positions.ToList());
			Employees = new ObservableCollection<Employee>(connection.Employees.ToList());

			Departments.Insert(0, new Department() { Name = "Все отделения" });
			Positions.Insert(0, new Position() { Name = "Все должности" });

			cbEmployeeDepartment.SetBinding(ItemsControl.ItemsSourceProperty, new Binding() {
				Source = Departments
			});
			cbEmployeePosition.SetBinding(ItemsControl.ItemsSourceProperty, new Binding() {
				Source = Positions
			});

			DataContext = this;
		}

		private void ApplyEmployeeFilter() {
			var view = CollectionViewSource.GetDefaultView(lvEmployees.ItemsSource);
			if (view == null) {
				return;
			}

			view.Filter = item => {
				Employee employee = item as Employee;
				if (employee == null) { return false; }

				if (selectedDepartment != null) {
					if (employee.Department1 != selectedDepartment) {
						return false;
					}
				}

				if (selectedPosition != null) {
					if (employee.Position1 != selectedPosition) {
						return false;
					}
				}

				if (searchText.Length > 0) {
					return employee.FullName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) > -1;
				}

				return true;
			};
		}

		private void ApplyEmployeeSorting() {
			var view = CollectionViewSource.GetDefaultView(lvEmployees.ItemsSource);
			if (view == null) {
				return;
			}

			view.SortDescriptions.Clear();

			if (selectedSorting != null) {
				view.SortDescriptions.Add(selectedSorting.Description);
			}
		}

		private void FilterDepartmentChanged(object sender, SelectionChangedEventArgs e) {
			selectedDepartment = null;

			ComboBox comboBox = sender as ComboBox;
			if (comboBox != null) {
				if (comboBox.SelectedIndex > 0) {
					selectedDepartment = comboBox.SelectedItem as Department;
				}
			}

			ApplyEmployeeFilter();
		}

		private void FilterPositionChanged(object sender, SelectionChangedEventArgs e) {
			selectedPosition = null;

			ComboBox comboBox = sender as ComboBox;
			if (comboBox != null) {
				if (comboBox.SelectedIndex > 0) {
					selectedPosition = comboBox.SelectedItem as Position;
				}
			}

			ApplyEmployeeFilter();
		}

		private void FilterSortingChanged(object sender, SelectionChangedEventArgs e) {
			selectedSorting = null;

			ComboBox comboBox = sender as ComboBox;
			if (comboBox != null) {
				if (comboBox.SelectedIndex > 0) {
					selectedSorting = comboBox.SelectedItem as SortingItem;
				}
			}

			ApplyEmployeeSorting();
		}

		private void SearchTextChanged(object sender, TextChangedEventArgs e) {
			searchText = "";

			TextBox textBox = sender as TextBox;
			if (textBox != null) {
				searchText = textBox.Text.Trim();
			}

			ApplyEmployeeFilter();
		}

		private void CreateNewEmployee(object sender, RoutedEventArgs e) {
			selectedEmployee = new Employee();

			spEmployee.DataContext = selectedEmployee;
			connection.Employees.Add(selectedEmployee);
			Employees.Add(selectedEmployee);

			lvEmployees.SelectedItem = selectedEmployee;

			bSaveEmployee.IsEnabled = true;
		}

		private void EmployeeChanged(object sender, SelectionChangedEventArgs e) {
			selectedEmployee = lvEmployees.SelectedItem as Employee;
			spEmployee.DataContext = selectedEmployee;
			if (selectedEmployee != null) {
				bSaveEmployee.IsEnabled = true;
			} else {
				bSaveEmployee.IsEnabled = false;
			}
		}

		private void SaveChanges(object sender, RoutedEventArgs e) {
			try {
				connection.SaveChanges();
				MessageBox.Show("Ок");
			} catch (Exception exception) {
				// а-ля обработка исключения..
				MessageBox.Show("Упс... " + exception.Message);
			}
		}
	}
}
