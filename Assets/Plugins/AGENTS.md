# Assets/Plugins / AGENTS

## 模块概览
`Assets/Plugins/` 放的是第三方插件代码，默认应视为只读区域。当前仓库里主要有：

- `Assets/Plugins/Sirenix/`
  - Odin Inspector / Odin Serializer
- `Assets/Plugins/ParadoxNotion/`
  - NodeCanvas / CanvasCore

## 工作原则
- 优先在 `Assets/Scripts/` 扩展功能，而不是直接修改 vendor 源码。
- 如果必须修改插件代码：
  - 只做最小范围改动
  - 记录插件来源、版本和修改原因
  - 预期会遇到兼容层、废弃 API、平台特化代码

## 当前易错点
- `ParadoxNotion` 目录里带有 `_DeprecatedFiles/`，不要把这些旧文件误认为项目自研逻辑。
- 第三方代码里存在大量编辑器兼容与序列化绕路逻辑，重构风险通常高于自研代码。
- 升级插件前先确认它和当前 Unity 版本（2021.3.37f1）以及现有资源/序列化格式是否兼容。
