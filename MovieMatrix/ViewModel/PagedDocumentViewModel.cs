using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DevExpress.Xpf.Data;
using MovieMatrix.Model;

namespace MovieMatrix.ViewModel
{
    public class PagedDocumentViewModel<T> : DocumentViewModel
    {
        public Func<int, CancellationToken, Task<List<T>>> FetchPage { get; set; }

        public PagedAsyncSource PageSource { get; private set; }

        public int PageCount { get; private set; }

        public void Refresh()
        {
            PageSource.PageIndex = 0;
            PageSource?.RefreshRows();
        }

        public PagedDocumentViewModel()
        {
            PageSource = new PagedAsyncSource
            {
                ElementType = typeof(T),
                PageNavigationMode = PageNavigationMode.ArbitraryWithTotalPageCount
            };

            PageSource.FetchPage += (s, e) =>
            {
                int page = (e.Skip / e.Take) + 1;
                e.Result = FetchRows(page, e.CancellationToken);
            };

            PageSource.GetTotalSummaries += (s, e) =>
            {
                for (int i = 0; i < e.Summaries.Length; i++)
                {
                    if (e.Summaries[i].SummaryType == SummaryType.Count)
                        e.Result = Task.FromResult(new object[] { PageCount });
                }
            };
        }

        private async Task<FetchRowsResult> FetchRows(int page, CancellationToken cancellationToken)
        {
            try
            {
                CancellationToken linkedToken = BackgroundOperation.Register(this, cancellationToken);
                if (page == 1)
                    await Task.Delay(500);
                
                List<T> result = await RunAsync(() => FetchPage.Invoke(page, linkedToken));
                PageCount = result.Capacity;
                PageSource.UpdateSummaries();

                return new FetchRowsResult(result.Cast<object>().ToArray());
            }
            catch(TaskCanceledException)
            {
                return new FetchRowsResult(new object[] { });
            }
            finally
            {
                BackgroundOperation.UnRegister(this);
            }
        }
    }
}
