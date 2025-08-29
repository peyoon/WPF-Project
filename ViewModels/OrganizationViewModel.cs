using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using WpfLogin.Helpers;
using WpfLogin.Models;

namespace WpfLogin.ViewModels
{
    // OrganizationViewModel은 INotifyPropertyChanged를 구현해야 합니다.
    public class OrganizationViewModel : INotifyPropertyChanged
    {
        // TreeView와 직접 바인딩되는 최종 데이터입니다.
        // UI가 변경사항을 자동으로 감지할 수 있도록 ObservableCollection을 사용합니다.
        public ObservableCollection<OrganizationNode> OrganizationTree { get; } = new ObservableCollection<OrganizationNode>();

        // 버튼과 연결되는 Command입니다.
        public ICommand LoadOrganizationChartCommand { get; }

        public OrganizationViewModel()
        {
            // MainViewModel에 있는 RelayCommand 클래스를 사용합니다.
            LoadOrganizationChartCommand = new RelayCommand(ExecuteLoadOrganizationChart);
        }

        private void ExecuteLoadOrganizationChart(object parameter)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv",
                Title = "조직도 CSV 파일 선택"
            };

            if (openFileDialog.ShowDialog() != true) return;

            try
            {
                string filePath = openFileDialog.FileName;
                var flatList = CsvReader.ReadOrganizationData(filePath);
                var nodeMap = flatList.ToDictionary(node => node.Id, node => node);

                // ==========================================================
                // [핵심 수정] 새로운 컬렉션을 만드는 대신, 기존 컬렉션을 수정합니다.

                // 1. 기존 트리를 깨끗하게 비웁니다.
                //    (이 작업만으로도 UI에 '모든 항목이 사라졌다'는 신호가 갑니다.)
                OrganizationTree.Clear();

                // 2. CSV 데이터로부터 새로운 트리를 조립합니다.
                var rootNodes = new List<OrganizationNode>();
                var allNodes = new Dictionary<int, OrganizationNode>();

                foreach (var node in flatList)
                {
                    allNodes[node.Id] = node;
                    if (node.ParentId == 0)
                    {
                        rootNodes.Add(node);
                    }
                }

                foreach (var node in flatList)
                {
                    if (node.ParentId != 0)
                    {
                        if (allNodes.TryGetValue(node.ParentId, out var parentNode))
                        {
                            parentNode.Children.Add(node);
                        }
                    }
                }

                // 3. 완성된 최상위 노드들을 하나씩 트리에 추가합니다.
                //    (항목이 추가될 때마다 UI에 '새 항목을 그려라'는 신호가 갑니다.)
                foreach (var rootNode in rootNodes)
                {
                    OrganizationTree.Add(rootNode);
                }
                // ==========================================================
            }
            catch (Exception ex)
            {
                MessageBox.Show($"파일 처리 중 오류 발생: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        // OnPropertyChanged는 이제 명시적으로 호출할 필요가 없습니다. ObservableCollection이 자동으로 처리합니다.
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}