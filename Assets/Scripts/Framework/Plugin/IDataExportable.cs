/// <summary>
/// 数据可导出插件接口（可选实现）
/// 实现此接口的插件在卸载/删除前，
/// 用户可选择导出其重要数据，重装后可恢复
/// 应用本体提供导出/导入流程，插件自定义具体数据
/// </summary>
public interface IDataExportable
{
    /// <summary>
    /// 导出插件重要数据为 JSON 字符串
    /// 由应用本体统一保存到导出目录
    /// </summary>
    /// <returns>JSON 格式的数据包</returns>
    string ExportData();

    /// <summary>
    /// 从 JSON 字符串恢复插件数据
    /// 重装插件后由应用本体调用
    /// </summary>
    /// <param name="jsonData">导出时的 JSON 数据包</param>
    /// <returns>是否恢复成功</returns>
    bool ImportData(string jsonData);

    /// <summary>
    /// 导出数据的人类可读描述（用于导出确认对话框）
    /// </summary>
    string ExportDescription { get; }
}
