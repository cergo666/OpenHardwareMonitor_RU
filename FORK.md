# Как устроен этот форк (и почему сборка больше не падает после слияния апстрима)

Форк собирает Open Hardware Monitor **без географической блокировки** по регионам,
которая зашита в проприетарную библиотеку апстрима.

## Что делает апстрим

Код `sergiye.Common` (темы, `NotifyIconAdv`, `Updater`, `OSHelper`, `ISettings`, ...)
апстрим берёт из одного из двух мест:

1. NuGet-пакеты `SergiyE.Common` / `SergiyE.Common.UI` (`"This is a private library,
   don't use it in your projects!"`) — именно в них сидит проверка региона
   (`"not compatible with your region"`).
2. С 23 окт. 2026 (коммиты `use UseLocalCommon files for build` в апстриме) в `.csproj`
   появился блок `UseLocalCommon`, который подключает те же файлы из
   `$(MSBuildThisFileDirectory)..\..\common\` — приватной выкладки **вне** репозитория,
   которой нет ни у кого, кроме автора.

## Что делает форк

* Собственные, «разблокированные» копии нужных классов лежат внутри проектов —
  `OpenHardwareMonitor\sergiye.Common\` и `OpenHardwareMonitorLib\sergiye.Common\`.
  SDK-проект подхватывает их автоматически, ссылки на пакеты `SergiyE.*` не нужны.
* `Directory.Build.targets` в корне репозитория (этого файла нет в апстриме) после
  обработки `.csproj`:
  * вырезает `PackageReference` на `SergiyE.Common` и `SergiyE.Common.UI`;
  * выкидывает `Compile`/`EmbeddedResource` из приватной папки `..\..\common\`,
    чтобы типы не объявлялись дважды;
  * падает с внятной ошибкой, если папки с копиями форка вдруг исчезли.

## Главное правило

`OpenHardwareMonitor/OpenHardwareMonitor.csproj` и
`OpenHardwareMonitorLib/OpenHardwareMonitorLib.csproj` должны быть **побайтово identical
с апстримом**. Весь форк-специфичный MSBuild-код — только в `Directory.Build.targets`.

Раньше форк удалял в этих файлах `PackageReference` на `SergiyE.*`, апстрим ту же самую
секцию переписал — отсюда вечный конфликт слияния (CI-задача синхронизации падала, и
почасовые релизы молча останавливались), а в `OpenHardwareMonitorLib.csproj` слияние
проходило «чисто» и молча возвращало приватный пакет: сборка валилась с
`error CS0433: тип "ISettings" существует как в "OpenHardwareMonitorLib", так и в
"SergiyE.Common"`.

CI следит за этим правилом сам (`build` job в `.github/workflows/auto-build.yml`):
проверяет, что `.csproj` совпадают с апстримом, что `SergiyE` нет в
`project.assets.json`, и что собранные `.exe` не содержат маркеров блокировки по
региону. Если синхронизация или сборка падают — заводится issue с меткой
`fork-sync-broken` и закрывается, когда всё снова зелёное.

## Обновление апстрима вручную

```bash
git fetch upstream master --tags
git merge --no-edit upstream/master   # конфликтов быть не должно
git push origin master
```

Если конфликт всё-таки есть и это только эти два `.csproj` — значит в них появилась
форк-правка, которой там быть не должно:

```bash
git checkout --theirs -- OpenHardwareMonitor/OpenHardwareMonitor.csproj OpenHardwareMonitorLib/OpenHardwareMonitorLib.csproj
git add -A && git commit
```

а саму правку перенести в `Directory.Build.targets`.

Единственный случай, который чинить придётся руками: апстрим начал использовать в своём
коде API из приватной библиотеки, которого нет в копиях форка (`CS0117`/`CS1061`). Тогда
в `*\sergiye.Common\` нужно добавить недостающий член класса, глядя, как его использует
код апстрима.
