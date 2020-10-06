# Changelog

All notable changes to the ToaruUnity.UI will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.2.6-preview] - 2020-10-06
### Added

- 添加`ToaruUnity.UI.Settings.ToaruUISettings`用于管理设置
- 添加`ToaruUnity.UI.ActionInfo`来提供更多关于`Action`的信息
- 添加`ToaruUnity.UI.ViewLoader`。`ViewLoader`只需要加载预制体，其余均由`UIManager`管理
- 添加了更多的事件
- 添加了部分的编辑器类型

### Changed

- `InjectActionCenterAttribute`重命名为`InjectActionsAttribute`
- `HandleActionCenterAttribute`重命名为`ActionAttribute`
- `Action`不再使用`Int32`类型的Id来区别，改用`String`类型的名称
- 将`AbstractView`的实现改为状态机，部分回调的返回值修改为`IEnumerator`类型
- 将`AbstractView.OnStateChanged`事件暴露在Inspector下

### Fixed

- 修复不注入`ActionCenter`时会报错的问题