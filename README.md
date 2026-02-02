# Excel Function Reader Service

Микросервис на .NET 8 для загрузки Excel-файлов с функциями подразделений, хранения строк в PostgreSQL и формирования агрегированного представления в Redis.

## Возможности
- Принимает `.xlsx` и `.xls` файлы.
- Каждая строка файла сохраняется в БД без агрегации и с историей загрузок.
- Возвращает статус импорта и JSON-результаты.
- Формирует агрегаты по `organization_code` + `code_structural_unit` и кэширует в Redis.

## Структура данных
Ожидаемый порядок колонок:
1. `organization_name`
2. `organization_code`
3. `structural_unit_name`
4. `code_structural_unit`
5. `function_code`
6. `function_description`

## Эндпоинты
- `POST /imports` — загрузка файла (multipart/form-data, поле `file`).
- `GET /imports/{id}/status` — статус импорта.
- `GET /imports/{id}/results` — JSON по строкам импорта + запись агрегатов в Redis.

## Redis-ключи
Агрегаты пишутся в `SET` по шаблону:
```
rsmv:org:{organization_code}:unit:{code_structural_unit}:functions
```
Каждое значение — уникальное `function_description`.

## Настройки
`appsettings.json` содержит строки подключения к PostgreSQL и Redis.
