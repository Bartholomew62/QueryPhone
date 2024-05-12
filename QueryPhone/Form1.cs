using QueryPhone.Clients;
using QueryPhone.Model;
using System.Text;

namespace QueryPhone
{
	public partial class Form1 : Form
	{
		private readonly IEnumerable<IQueryPhoneClient> _queryPhoneClients;

		public Form1(IEnumerable<IQueryPhoneClient> queryPhoneClients)
		{
			InitializeComponent();
			_queryPhoneClients = queryPhoneClients;
			foreach (var ins in _queryPhoneClients)
			{
				int i = clientsCheckedListBox.Items.Add(ins.GetName());
				clientsCheckedListBox.SetItemChecked(i, true);
			}
		}

		private async void btnQueryPhone_Click(object sender, EventArgs e)
		{
			string phone = txtPhone.Text;
			if (string.IsNullOrEmpty(phone))
			{
				MessageBox.Show("Ոݔ��̖�a");
				return;
			}

			var checkedItemNames = clientsCheckedListBox.CheckedItems.OfType<string>();
			var checkedClients = _queryPhoneClients.Join(checkedItemNames, ins => ins.GetName(), name => name, (ins, name) => ins);
			if (!checkedClients.Any())
			{
				MessageBox.Show("Ո�xȡ��ԃ��Դ");
				return;
			}

			txtResult.Clear();
			var tasks = checkedClients.Select(client =>
				Task.Run(async () => 
				{
					return PrintResult(client.GetName(), await client.QueryAsync(phone)); 
				})).ToList();

			while (tasks.Count > 0)
			{
				var completedTask = await Task.WhenAny(tasks);
				tasks.Remove(completedTask);
				txtResult.AppendText(await completedTask);
			}
		}

		private string PrintResult(string name, QueryPhoneResult result)
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine($"-------{name}-------");
			sb.AppendLine($"Url��{result.QueryUrl}");
			if (!result.Success)
			{
				sb.AppendLine("��ԃ�e�`");
				return sb.ToString();
			}

			sb.AppendLine("���Ñ��؈�");
			sb.AppendLine(string.Join(Environment.NewLine, result.ReportMsgs));
			if (result.ReportMsgs.Any())
				sb.AppendLine();
			sb.AppendLine("���u��");
			sb.AppendLine(string.Join(Environment.NewLine, result.SummaryMsgs));
			sb.AppendLine();
			return sb.ToString();
		}
	}
}
