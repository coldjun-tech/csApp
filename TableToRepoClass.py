from pathlib import Path
import pymysql


def build_params_block(table_name: str, columns: list[dict]) -> str:
    """단일 테이블의 TDataParameter[] 코드 블록을 생성합니다."""
    lines = [
        f"// ==================== {table_name} ====================",
        "TDataParameter[] vParams = new TDataParameter[] {",
    ]

    for col in columns:
        col_name = col["COLUMN_NAME"]
        edit_name = (col["COLUMN_COMMENT"] or "").strip()
        lines.append(
            f'    new TDataParameter("@{col_name}", MySqlDbType.String, edt{edit_name}.EditValue),'
        )

    lines.append("};")
    return "\n".join(lines)


def generate_all_update_params_to_text(
    db_config: dict,
    database_name: str,
    output_file: str = "uvf_db_update_params.txt",
) -> str:
    """DB 내 모든 테이블의 TDataParameter[] 코드를 하나의 텍스트 파일로 저장합니다."""
    tables_query = """
        SELECT TABLE_NAME
          FROM INFORMATION_SCHEMA.TABLES
         WHERE TABLE_SCHEMA = %s
           AND TABLE_TYPE = 'BASE TABLE'
         ORDER BY TABLE_NAME;
    """

    columns_query = """
        SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, COLUMN_COMMENT
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
                blocks.append(build_params_block(table_name, columns))

    full_text = "\n\n".join(blocks) + "\n"

    out_path = Path(output_file)
    out_path.write_text(full_text, encoding="utf-8")
    print(f"총 {len(tables)}개 테이블의 파라미터 코드를 '{out_path.resolve()}' 파일에 저장했습니다.")

    return full_text


if __name__ == "__main__":
    db_config = {
        "host": "localhost",
        "user": "root",
        "password": "1122qqww",
        "database": "uvf_db",
        "charset": "utf8mb4",
    }

    target_database = "uvf_db"
    output_txt = "uvf_db_update_params.txt"

    result_text = generate_all_update_params_to_text(
        db_config=db_config,
        database_name=target_database,
        output_file=output_txt,
    )

    # 콘솔에서 바로 확인하려면 아래 주석을 해제하세요.
    # print(result_text)