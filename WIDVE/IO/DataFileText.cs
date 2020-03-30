using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace WIDVE.IO
{
	public abstract class DataFileText : DataFile
	{
		public sealed override FileFormats FileFormat=> FileFormats.Text;

		/// <summary>
		/// Returns a StreamWriter with the file's specified write mode.
		/// </summary>
		/// <returns>New StreamWriter for this file.</returns>
		public StreamWriter GetWriter()
		{
			if (WriteMode == WriteModes.ReadOnly) return null;

			else
			{
				bool append = !ShouldOverwrite(WriteMode, TimesOpened);
				TimesOpened++;
				return new StreamWriter(Path, append);
			}
		}

		/// <summary>
		/// Returns a StreamReader for the file.
		/// </summary>
		/// <returns>New StreamReader for this file.</returns>
		public StreamReader GetReader()
		{
			return new StreamReader(Path);
		}

		/// <summary>
		/// Write text to the file.
		/// </summary>
		/// <param name="text">String data of any format.</param>
		/// /// <param name="flush">Optional: flush stream after writing.</param>
		public virtual void Write(string text, bool flush=true)
		{
			using (StreamWriter streamWriter = GetWriter())
			{
				if (streamWriter == null) return;

				streamWriter.Write(text);

				if (flush) streamWriter.Flush();
			}
		}

		/// <summary>
		/// Write text to the file.
		/// </summary>
		/// <param name="text">Array of string data.</param>
		/// <param name="separator">Optional: separator between each string.</param>
		/// <param name="flush">Optional: flush stream after writing.</param>
		public virtual void Write(string[] text, string separator = "", bool flush = true)
		{
			using (StreamWriter streamWriter = GetWriter())
			{
				if (streamWriter == null) return;

				for (int i = 0; i < text.Length; i++)
				{
					streamWriter.Write(text[i]);
					if(!string.IsNullOrEmpty(separator)) streamWriter.Write(separator);
				}

				if (flush) streamWriter.Flush();
			}
		}
	}
}