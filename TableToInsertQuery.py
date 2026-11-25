from pathlib import Path
import pymysql


def get_mysql_db_type(data_type: str, column_type: str = "") -> str:
    """DB 컬럼 타입을 C# MySqlDbType 이름으로 변환합니다."""
    type_map = {
        "tinyint": "Byte", "smallint": "Int16", "mediumint": "Int24",
        "int": "Int32", "integer": "Int32", "bigint": "Int64",
        "bit": "Bit", "bool": "Bool", "boolean": "Bool",
        "decimal": "Decimal", "numeric": "Decimal",
        "float": "Float", "double": "Double", "real": "Double",
        "date": "Date", "datetime": "DateTime", "timestamp": "Timestamp",
        "time": "Time", "year": "Year",
        "binary": "Binary", "varbinary": "VarBinary",
        "tinyblob": "TinyBlob", "blob": "Blob",
        "mediumblob": "MediumBlob", "longblob": "LongBlob",
    }
    result = type_map.get(data_type.lower(), "String")
    if "unsigned" in column_type.lower():
        result = {
            "Byte": "UByte", "Int16": "UInt16", "Int24": "UInt24",
            "Int32": "UInt32", "Int64": "UInt64",
        }.get(result, result)
    return result


def build_insert_block(
    table_name: str,
    columns: list[dict],
    value_expressions: dict[str, str] | None = None,
) -> str:
    """컬럼별 INSERT 쿼리와 aItem 값을 사용하는 TDataParam 배열을 생성합니다."""
    if not columns:
        raise ValueError(f"'{table_name}' 테이블에 삽입할 컬럼이 없습니다.")

    def append_line(sql: str) -> str:
        escaped = (sql.replace("\\", "\\\\").replace('"', '\\"')
                   .replace("\r", "\\r").replace("\n", "\\n"))
        return f'sbQuery.Append("{escaped}");'

    lines = [
        "StringBuilder sbQuery = new StringBuilder();",
        "sbQuery.Clear();",
        append_line(f"INSERT INTO {table_name} ( "),
    ]

    for index, col in enumerate(columns):
        prefix = " " if index == 0 else ", "
        lines.append(append_line(f"{prefix}{col['COLUMN_NAME']}"))

    lines.append(append_line(" ) VALUES ( "))

    for index, col in enumerate(columns):
        prefix = " " if index == 0 else ", "
        lines.append(append_line(f"{prefix}@{col['COLUMN_NAME']}"))

    lines.append(append_line(" );"))
    lines.extend(["", "TDataParam[] vParams = new TDataParam[] {"])
    value_expressions = value_expressions or {}
    for index, col in enumerate(columns):
        name = col["COLUMN_NAME"]
        db_type = get_mysql_db_type(col["DATA_TYPE"], col.get("COLUMN_TYPE", ""))
        value = value_expressions.get(name, f"aItem.{name}")
        comma = "," if index < len(columns) - 1 else ""
        parameter_name = (name.replace("\\", "\\\\").replace('"', '\\"')
                          .replace("\r", "\\r").replace("\n", "\\n"))
        lines.append(
            f'    new TDataParam("@{parameter_name}", MySqlDbType.{db_type}, {value}){comma}'
        )
    lines.append("};")
    return "\n".join(lines)


def generate_all_insert_queries_to_text(
    db_config: dict,
    database_name: str,
    output_file: str = "db_insert_queries.txt",
    value_expressions: dict[str, str] | None = None,
) -> str:
    """기본 키 유무와 관계없이 모든 테이블의 INSERT 코드를 저장합니다."""
    tables_query = """
        SELECT TABLE_NAME
          FROM INFORMATION_SCHEMA.TABLES
         WHERE TABLE_SCHEMA = %s
           AND TABLE_TYPE = 'BASE TABLE'
         ORDER BY TABLE_NAME;
    """
    columns_query = """
        SELECT COLUMN_NAME, DATA_TYPE, COLUMN_TYPE
          FROM INFORMATION_SCHEMA.COLUMNS
         WHERE TABLE_SCHEMA = %s AND TABLE_NAME = %s
         ORDER BY ORDINAL_POSITION;
    """

    blocks = []
    with pymysql.connect(**db_config) as connection:
        with connection.cursor(pymysql.cursors.DictCursor) as cursor:
            cursor.execute(tables_query, (database_name,))
            tables = [row["TABLE_NAME"] for row in cursor.fetchall()]

            if not tables:
                print(f"데이터베이스 '{database_name}'에서 테이블을 찾을 수 없습니다.")
                return ""

            for table_name in tables:
                cursor.execute(columns_query, (database_name, table_name))
                columns = cursor.fetchall()
                try:
                    block = build_insert_block(table_name, columns, value_expressions)
                except ValueError as error:
                    print(f"건너뜀: {error}")
                    continue
                blocks.append(f"// ==================== {table_name} ====================\n{block}")

    full_text = "\n\n".join(blocks) + ("\n" if blocks else "")
    out_path = Path(output_file)
    out_path.write_text(full_text, encoding="utf-8")
    print(f"총 {len(blocks)}개 테이블의 INSERT 코드를 '{out_path.resolve()}' 파일에 저장했습니다.")
    return full_text


if __name__ == "__main__":
    # TableToModelClass.py와 같은 DB 접속 설정을 사용합니다.
    db_config = {
        "host": "localhost",
        "user": "root",
        "password": "1122qqww",
        "database": "tdd_db",
        "charset": "utf8mb4",
    }

    target_database = "tdd_db"
    output_txt = "tdd_db_insert_queries.txt"
    # 별도 변환이나 다른 속성명이 필요한 컬럼의 C# 값 표현식입니다.
    # 이 사전에 없는 컬럼은 aItem.컬럼명을 사용합니다.
    value_expressions = {
        "PressVals1": "GetPressValues(aItem.PresValues1, aItem.PressVals1)",
        "PressVals2": "GetPressValues(aItem.PresValues2, aItem.PressVals2)",
        "PressChtImg": "aItem.PresChtImg",
    }

    result_text = generate_all_insert_queries_to_text(
        db_config=db_config,
        database_name=target_database,
        output_file=output_txt,
        value_expressions=value_expressions,
    )

    print(result_text)
