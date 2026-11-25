from pathlib import Path
import pymysql


def get_csharp_type(mysql_type: str, is_nullable: bool) -> str:
    """MySQL 데이터 타입을 C# 데이터 타입으로 변환합니다."""
    type_map = {
        "int": "int",
        "integer": "int",
        "bigint": "long",
        "smallint": "short",
        "tinyint": "byte",      # tinyint(1)을 bool로 쓰신다면 "bool"로 변경하세요.
        "bit": "bool",
        "decimal": "decimal",
        "numeric": "decimal",
        "float": "float",
        "double": "double",
        "date": "DateTime",
        "datetime": "DateTime",
        "timestamp": "DateTime",
        "char": "string",
        "varchar": "string",
        "text": "string",
        "longtext": "string",
        "blob": "byte[]",
        "longblob": "byte[]",
    }

    csharp_type = type_map.get(mysql_type.lower(), "string")

    if is_nullable and csharp_type not in ("string", "byte[]"):
        csharp_type += "?"

    return csharp_type


def build_class_block(table_name: str, columns: list[dict], indent: str = "") -> str:
    """단일 테이블의 C# 클래스 블록 문자열을 생성합니다."""
    lines = [
        f"{indent}public class {table_name}",
        f"{indent}{{",
    ]

    for col in columns:
        col_name = col["COLUMN_NAME"]
        data_type = col["DATA_TYPE"]
        is_nullable = col["IS_NULLABLE"] == "YES"
        csharp_type = get_csharp_type(data_type, is_nullable)
        field_name = f"_{col_name[0].lower()}{col_name[1:]}"
        lines.extend([
            f"{indent}    private {csharp_type} {field_name};",
            f'{indent}    [DbColumn("{col_name}")]',
            f'{indent}    [DisplayName("{col_name}")]',
            f"{indent}    public {csharp_type} {col_name}",
            f"{indent}    {{",
            f"{indent}        get => {field_name};",
            f"{indent}        set => SetProperty(ref {field_name}, value);",
            f"{indent}    }}",
            "",
        ])

    lines.append(f"{indent}}}")
    return "\n".join(lines)


def generate_all_models_to_single_text(
    db_config: dict,
    database_name: str,
    output_file: str = "db_models.txt",
    namespace: str = "",
) -> str:
    """DB 내 모든 테이블을 하나의 텍스트로 합쳐 저장하고 결과를 반환합니다."""
    tables_query = """
        SELECT TABLE_NAME
          FROM INFORMATION_SCHEMA.TABLES
         WHERE TABLE_SCHEMA = %s
           AND TABLE_TYPE = 'BASE TABLE'
         ORDER BY TABLE_NAME;
    """

    columns_query = """
        SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
          FROM INFORMATION_SCHEMA.COLUMNS
         WHERE TABLE_SCHEMA = %s AND TABLE_NAME = %s
         ORDER BY ORDINAL_POSITION;
    """

    indent = "    " if namespace else ""
    class_blocks = []

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
                class_blocks.append(build_class_block(table_name, columns, indent=indent))

    # 전체 텍스트 조합
    parts = ["using System;", "using System.ComponentModel;", ""]
    if namespace:
        parts.append(f"namespace {namespace}")
        parts.append("{")
        parts.append("\n\n".join(class_blocks))
        parts.append("}")
    else:
        parts.append("\n\n".join(class_blocks))

    full_text = "\n".join(parts) + "\n"

    # 단일 텍스트 파일로 저장
    out_path = Path(output_file)
    out_path.write_text(full_text, encoding="utf-8")
    print(f"총 {len(tables)}개 테이블을 '{out_path.resolve()}' 파일에 저장했습니다.")

    return full_text


if __name__ == "__main__":
    db_config = {
        "host": "localhost",
        "user": "root",
        "password": "1122qqww",
        "database": "tdd_db",
        "charset": "utf8mb4",
    }

    target_database = "tdd_db"
    output_txt = "tdd_db_models.txt" # 저장할 단일 텍스트 파일명
    target_namespace = ""  # 필요 없으면 "" 로 비워두세요.

    result_text = generate_all_models_to_single_text(
        db_config=db_config,
        database_name=target_database,
        output_file=output_txt,
        namespace=target_namespace,
    )

    # 콘솔에서도 바로 확인하고 싶다면 아래 주석을 해제하세요.
    print(result_text)
