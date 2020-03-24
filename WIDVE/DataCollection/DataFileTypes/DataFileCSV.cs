using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using WIDVE.Utilities;

namespace WIDVE.DataCollection
{
	[CreateAssetMenu(fileName = nameof(DataFileCSV), menuName = nameof(DataFile) + "/" + nameof(DataFileCSV), order = WIDVEEditor.C_ORDER)]
	public class DataFileCSV : DataFileText
	{
		public override string Extension => "csv";

		public virtual char Separator => ',';

		public virtual char NewLine => '\n';

		public override void WriteData(DataContainer[][] buffer)
		{
			try
			{   
				//write buffered data in order
				using (StreamWriter streamWriter = GetWriter())
				{
					if (streamWriter == null) return;

					for (int i = 0; i < buffer.Length; i++)
					{   
						//write each set of DataContainers as a single line
						DataContainer[] dataContainers = buffer[i];
						if (dataContainers != null)
						{   
							//write data from each container with correct separator
							for (int j = 0; j < dataContainers.Length; j++)
							{   
								string dataString = dataContainers[j].DataToString();

								streamWriter.Write(dataString);
								
								//write separator, unless at end of line
								if (j < dataContainers.Length - 1) streamWriter.Write(Separator);
							}

							//reached end of line
							streamWriter.Write(NewLine);
						}
					}

					//done writing to this file
					streamWriter.Flush();
				}
			}
			catch (IOException ioe)
			{
				Debug.LogError($"Error! Could not open {Filename} for writing [{ioe}].");
			}
		}
	}
}