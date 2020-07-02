using System;
using System.IO;
using System.Reflection;
using Common.Services.ApplicationServices;
using Services.Broadcast.Entities;

namespace Services.Broadcast.ApplicationServices
{
	public interface IHealthService : IApplicationService
	{
		/// <summary>
		///     Gets all Health Info.
		/// </summary>
		/// <returns></returns>
		HealthResponseDto GetInfo(Assembly assembly);
	}

	public class HealthService : IHealthService
	{
		public HealthResponseDto GetInfo(Assembly executingAssembly)
		{
			var dto = new HealthResponseDto();

			var localPath = _GetExecutingAssemblyLocalPath(executingAssembly);

			dto.ApiBuildContent = File.ReadAllText(Path.Combine(localPath, "api_build.txt"));
			var assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (var assembly in assemblies)
			{
				if (assembly.FullName.Contains("Services.Broadcast"))
				{
					dto.DependentAssemblyName = assembly.ManifestModule.Name;
					var dependentAssemblyTime = GetLinkerTimestampUtc(assembly);
					dto.DependentCreationTime =
						$"{dependentAssemblyTime:MM/dd/yyyy hh:mm:ss tt} {_GetTimeStampName(dependentAssemblyTime)}";
					dto.DependentAssemblyVersion = assembly.GetName().Version.ToString();
				}

				if (assembly.FullName.Contains(executingAssembly.GetName().Name))
				{
					dto.ExecutingAssemblyName = assembly.ManifestModule.Name;
					var executingAssemblyTime = GetLinkerTimestampUtc(assembly);
					dto.ExecutingAssemblyCreationTime =
						$"{executingAssemblyTime:MM/dd/yyyy hh:mm:ss tt} {_GetTimeStampName(executingAssemblyTime)}";
					dto.ExecutingAssemblyVersion = assembly.GetName().Version.ToString();
				}
			}

			return dto;
		}

		protected string _GetTimeStampName(DateTime dt)
		{
			return TimeZone.CurrentTimeZone.IsDaylightSavingTime(dt)
				? TimeZone.CurrentTimeZone.DaylightName
				: TimeZone.CurrentTimeZone.StandardName;
		}

		protected virtual string _GetExecutingAssemblyLocalPath(Assembly executingAssembly)
		{
			return new Uri(Path.GetDirectoryName(executingAssembly.GetName().CodeBase)).LocalPath;
		}

		public static DateTime GetLinkerTimestampUtc(Assembly assembly)
		{
			var location = assembly.Location;
			return GetLinkerTimestampUtc(location);
		}

		public static DateTime GetLinkerTimestampUtc(string filePath)
		{
			const int peHeaderOffset = 60;
			const int linkerTimestampOffset = 8;
			var bytes = new byte[2048];

			using (var file = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			{
				file.Read(bytes, 0, bytes.Length);
			}

			var headerPos = BitConverter.ToInt32(bytes, peHeaderOffset);
			var secondsSince1970 = BitConverter.ToInt32(bytes, headerPos + linkerTimestampOffset);
			var dt = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			return TimeZoneInfo.ConvertTime(dt.AddSeconds(secondsSince1970), TimeZoneInfo.Local);
		}
	}
}