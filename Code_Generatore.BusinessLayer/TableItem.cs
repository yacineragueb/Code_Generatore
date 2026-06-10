

using System.ComponentModel;

namespace Code_Generatore.BusinessLayer
{
    public class TableItem : INotifyPropertyChanged
    {
        private bool _isSelected;
        public string TableName { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected == value)
                    return;

                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        public TableItem(string tableName)
        {
            TableName = tableName;
        }

        private void OnPropertyChanged(string  propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
