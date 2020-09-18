using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace Kogane.Internal
{
	/// <summary>
	/// Addressable Asset System で Path が Missing File になっているエントリが存在しないか確認するエディタ拡張
	/// </summary>
	internal static class AddressablesMissingFileChecker
	{
		//================================================================================
		// 定数
		//================================================================================
		private const string TITLE             = "UniAddressablesMissingFileChecker";
		private const string MESSAGE_NOT_EXIST = "Missing File になっているエントリは存在しませんでした";
		private const string MESSAGE_EXIST     = "Missing File になっているエントリが見つかりました。\nこれらを削除しますか？";
		private const string MESSAGE_COMPLETE  = "Missing File になっているエントリをすべて削除しました";
		private const string LOG_EXIST         = "[" + TITLE + "] Missing File になっているエントリ";

		//================================================================================
		// 関数（static）
		//================================================================================
		/// <summary>
		/// Path が Missing File になっているエントリを検索します
		/// </summary>
		[MenuItem( "Edit/UniAddressablesMissingFileChecker/Path が Missing File になっているエントリを検索" )]
		private static void Check()
		{
			var settings      = AddressableAssetSettingsDefaultObject.Settings;
			var allAssetPaths = new HashSet<string>( AssetDatabase.GetAllAssetPaths() );

			var missingEntries = settings.groups
					.SelectMany( c => c.entries )
					.Where( c => c.address != "Scenes In Build" && c.address != "*/Resources/" )
					.Where( c => !allAssetPaths.Contains( c.AssetPath ) )
					.ToArray()
				;

			if ( missingEntries.Length <= 0 )
			{
				EditorUtility.DisplayDialog( TITLE, MESSAGE_NOT_EXIST, "OK" );
				return;
			}

			var builder = new StringBuilder();
			builder.AppendLine( LOG_EXIST );

			foreach ( var entry in missingEntries )
			{
				builder.AppendLine( entry.address );
			}

			if ( !EditorUtility.DisplayDialog( TITLE, MESSAGE_EXIST, "はい", "いいえ" ) )
			{
				Debug.LogError( builder.ToString() );
				return;
			}

			foreach ( var entry in missingEntries )
			{
				var parentGroup = entry.parentGroup;
				parentGroup.RemoveAssetEntry( entry );
				settings.SetDirty( AddressableAssetSettings.ModificationEvent.EntryRemoved, entry, false );
			}

			AssetDatabase.SaveAssets();

			EditorUtility.DisplayDialog( TITLE, MESSAGE_COMPLETE, "OK" );

			Debug.Log( builder.ToString() );
		}
	}
}