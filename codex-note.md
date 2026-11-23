# WPF 고철 배합 데이터의 마스터·디테일 설계

## 목적과 적용 범위

고철 배합 테이블의 반복 컬럼을 계층형 Model로 표현하고, WPF MVVM 구조에서 조회·수정·저장하는 방법을 정리한다.

기존 DB의 가로형 컬럼은 유지한다. Repository와 Mapper에서 DB 행과 계층형 Model을 변환하므로, 이 설계를 적용하기 위해 DB 테이블을 먼저 분리할 필요는 없다.

이 문서는 설계 및 구현 예시다. 실제 프로젝트 소스 변경, DB 스키마 변경, DB 연결 및 빌드 검증은 수행하지 않았다.

## 첨부 컬럼의 반복 구조

배합 하나에 장입 2개가 있고, 각 장입에는 원료 항목 40개가 반복된다.

```text
고철 배합 Master
 ├─ 배합번호
 ├─ 장입합계
 ├─ 비고
 └─ 장입 목록: 2개
      ├─ 장입차수
      ├─ 장입 TOTAL
      ├─ Basket 횟수
      ├─ Chute 횟수
      ├─ 전력원단위
      └─ 원료 Detail 목록: 40개
           ├─ 순번
           ├─ 번호
           ├─ 구분
           ├─ CODE
           ├─ 품명
           └─ 예정량
```

| 구분 | 기존 DB 컬럼 | Model 경로 |
|---|---|---|
| 배합번호 | `sScrapno_001` | `ScrapMix.ScrapNo` |
| 장입합계 | `iPuttotX_002` | `ScrapMix.TotalAmount` |
| 비고 | `sContent_411` | `ScrapMix.Remark` |
| 1장입 TOTAL | `iPuttot1_003` | `Charges[0].TotalAmount` |
| 2장입 TOTAL | `iPuttot2_204` | `Charges[1].TotalAmount` |
| 1장입 Basket / Chute 횟수 | `iBasketCount1_405` / `iChuteCount1_406` | `Charges[0].BasketCount` / `ChuteCount` |
| 2장입 Basket / Chute 횟수 | `iBasketCount2_407` / `iChuteCount2_408` | `Charges[1].BasketCount` / `ChuteCount` |
| 1장입 전력원단위 | `iPower01_409` | `Charges[0].PowerUnit` |
| 2장입 전력원단위 | `iPower02_410` | `Charges[1].PowerUnit` |

원료 항목은 다음 패턴으로 반복된다.

| 장입 | 원료 순번 | 번호 컬럼 | 예정량 컬럼 |
|---|---:|---|---|
| 1 | 1 | `sScNo101_004` | `iPlan101_008` |
| 1 | 40 | `sScNo140_199` | `iPlan140_203` |
| 2 | 1 | `sScNo201_205` | `iPlan201_209` |
| 2 | 40 | `sScNo240_400` | `iPlan240_404` |

각 항목의 컬럼 순서는 번호(`sScNo`), 구분(`sScDv`), CODE(`iCode`), 품명(`sName`), 예정량(`iPlan`)이다.

## 구현 전제

- `CommunityToolkit.Mvvm`과 `Dapper`를 사용한다.
- 예시 SQL은 SQL Server 기준이며, 테이블명 `te_scrap_mix`는 실제 테이블명으로 교체해야 한다.
- `sScrapno_001`은 배합을 유일하게 식별하는 키라고 가정한다. 실제 키가 복합키라면 조회·저장 조건을 함께 수정해야 한다.
- 첨부에는 자료형과 NULL 허용 여부가 없으므로, `i` 컬럼은 NULL 없는 `int`, `s` 컬럼은 NULL 없는 문자열로 가정한다.
- 합계는 DB 값을 유지한다. 예정량 합산 여부, 단위, 반올림 등 업무 규칙이 확정된 뒤 합계 재계산을 추가한다.
- 예시는 C# 11 이상 문법인 raw string literal을 사용한다. 이전 언어 버전에서는 일반 문자열 또는 verbatim 문자열로 변경한다.
- 코드의 네임스페이스와 프로젝트 내 `using`은 실제 구성에 맞게 추가한다.

## 역할 분리

| 구성 요소 | 책임 |
|---|---|
| Model | 배합·장입·원료 데이터와 속성 변경 알림 |
| ViewModel | 현재 배합, 선택한 장입, 조회·저장 명령, 처리 상태와 메시지 |
| Repository | 배합 단위의 DB 조회·저장 |
| Mapper | 가로형 DB 컬럼과 계층형 Model 사이 변환 |
| View | 바인딩을 통한 데이터 표시·편집과 명령 실행 |

Model이 변경 알림을 제공한다고 해서 ViewModel이 되는 것은 아니다. 화면에서는 ViewModel을 `DataContext`로 연결하고, ViewModel이 제공하는 Model의 하위 속성에 바인딩한다.

권장 파일 구성은 다음과 같다.

```text
Models/
  ScrapMix.cs
  ScrapCharge.cs
  ScrapMaterial.cs
Repositories/
  IScrapMixRepository.cs
  ScrapMixRepository.cs
  ScrapMixMapper.cs
ViewModels/
  ScrapMixViewModel.cs
Views/
  ScrapMixView.xaml
  ScrapMixView.xaml.cs
```

## 1. Model: 배합 / 장입 / 원료

앞서 사용한 `TModelItem_ScPatrg`와는 데이터 구성이 다르므로 고철 배합용 이름을 사용한다.

```csharp
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

// Master: 고철 배합
public partial class ScrapMix : ObservableObject
{
    public string ScrapNo { get; init; } = "";

    [ObservableProperty]
    private int totalAmount;

    [ObservableProperty]
    private string remark = "";

    public IReadOnlyList<ScrapCharge> Charges { get; } =
        Enumerable.Range(1, 2)
            .Select(number => new ScrapCharge(number))
            .ToList()
            .AsReadOnly();
}

// 장입 단위
public partial class ScrapCharge : ObservableObject
{
    public int ChargeNo { get; }

    [ObservableProperty]
    private int totalAmount;

    [ObservableProperty]
    private int basketCount;

    [ObservableProperty]
    private int chuteCount;

    [ObservableProperty]
    private int powerUnit;

    public IReadOnlyList<ScrapMaterial> Materials { get; }

    public ScrapCharge(int chargeNo)
    {
        ChargeNo = chargeNo;

        Materials = Enumerable.Range(1, 40)
            .Select(number => new ScrapMaterial(number))
            .ToList()
            .AsReadOnly();
    }
}

// Detail: 장입별 원료 항목
public partial class ScrapMaterial : ObservableObject
{
    public int Sequence { get; }

    [ObservableProperty]
    private string scrapItemNo = "";

    [ObservableProperty]
    private string category = "";

    [ObservableProperty]
    private int code;

    [ObservableProperty]
    private string name = "";

    [ObservableProperty]
    private int plannedAmount;

    public ScrapMaterial(int sequence)
    {
        Sequence = sequence;
    }
}
```

`Charges`와 `Materials`는 각각 장입 2개와 원료 슬롯 40개로 고정한다. 목록의 항목 추가·삭제는 없으므로 읽기 전용 목록을 사용한다. 각 항목의 속성은 수정 가능하며 `ObservableObject`가 변경을 알린다.

Model 속성과 DB 컬럼이 일대일로 대응하지 않으므로 여기에는 `[DbTable]`, `[DbColumn]`을 붙이지 않는다. 기존 매핑 기반 클래스를 사용한다면 계층형 목록을 자동 저장할 수 있는지 확인하고, 자동 매핑과 아래 Mapper의 책임을 중복시키지 않는다.

## 2. Mapper: 반복 컬럼 변환

반복 컬럼의 이름과 인덱스 계산을 한곳에서 관리한다.

```csharp
using System;
using System.Collections.Generic;

public static class ScrapMixMapper
{
    private static string Column(
        string prefix, int chargeNo, int sequence, int offset)
    {
        // 1장입 첫 컬럼: 004, 2장입 첫 컬럼: 205
        int start = chargeNo == 1 ? 4 : 205;
        int index = start + (sequence - 1) * 5 + offset;

        return $"{prefix}{chargeNo}{sequence:00}_{index:000}";
    }

    private static string TotalColumn(int chargeNo) =>
        chargeNo == 1 ? "iPuttot1_003" : "iPuttot2_204";

    private static string BasketColumn(int chargeNo) =>
        chargeNo == 1 ? "iBasketCount1_405" : "iBasketCount2_407";

    private static string ChuteColumn(int chargeNo) =>
        chargeNo == 1 ? "iChuteCount1_406" : "iChuteCount2_408";

    private static string PowerColumn(int chargeNo) =>
        chargeNo == 1 ? "iPower01_409" : "iPower02_410";

    private static object Required(
        IDictionary<string, object> row, string column)
    {
        if (!row.TryGetValue(column, out var value) ||
            value is null || value is DBNull)
        {
            throw new InvalidOperationException(
                $"필수 컬럼 값이 없습니다: {column}");
        }

        return value;
    }

    // DB의 한 행 → 계층형 Model
    public static ScrapMix FromRow(IDictionary<string, object> row)
    {
        string Text(string column) =>
            Convert.ToString(Required(row, column))!;

        int Number(string column) =>
            Convert.ToInt32(Required(row, column));

        var mix = new ScrapMix
        {
            ScrapNo = Text("sScrapno_001"),
            TotalAmount = Number("iPuttotX_002"),
            Remark = Text("sContent_411")
        };

        foreach (var charge in mix.Charges)
        {
            int c = charge.ChargeNo;

            charge.TotalAmount = Number(TotalColumn(c));
            charge.BasketCount = Number(BasketColumn(c));
            charge.ChuteCount = Number(ChuteColumn(c));
            charge.PowerUnit = Number(PowerColumn(c));

            foreach (var material in charge.Materials)
            {
                int n = material.Sequence;

                material.ScrapItemNo = Text(Column("sScNo", c, n, 0));
                material.Category = Text(Column("sScDv", c, n, 1));
                material.Code = Number(Column("iCode", c, n, 2));
                material.Name = Text(Column("sName", c, n, 3));
                material.PlannedAmount = Number(Column("iPlan", c, n, 4));
            }
        }

        return mix;
    }

    // 계층형 Model → DB 컬럼별 값
    public static Dictionary<string, object> ToColumns(ScrapMix mix)
    {
        var values = new Dictionary<string, object>
        {
            ["sScrapno_001"] = mix.ScrapNo,
            ["iPuttotX_002"] = mix.TotalAmount,
            ["sContent_411"] = mix.Remark
        };

        foreach (var charge in mix.Charges)
        {
            int c = charge.ChargeNo;

            values[TotalColumn(c)] = charge.TotalAmount;
            values[BasketColumn(c)] = charge.BasketCount;
            values[ChuteColumn(c)] = charge.ChuteCount;
            values[PowerColumn(c)] = charge.PowerUnit;

            foreach (var material in charge.Materials)
            {
                int n = material.Sequence;

                values[Column("sScNo", c, n, 0)] = material.ScrapItemNo;
                values[Column("sScDv", c, n, 1)] = material.Category;
                values[Column("iCode", c, n, 2)] = material.Code;
                values[Column("sName", c, n, 3)] = material.Name;
                values[Column("iPlan", c, n, 4)] = material.PlannedAmount;
            }
        }

        return values;
    }
}
```

빈 슬롯을 포함해 원료 40개를 모두 읽고 저장한다. 사용자가 원료를 비우면 기존 DB 값도 갱신되어야 하기 때문이다.

실제 컬럼이 NULL을 허용하면 `Required()`와 Model 자료형을 해당 정책에 맞게 변경한다. NULL과 0 또는 빈 문자열의 의미를 임의로 같게 취급하지 않는다.

## 3. Repository: 배합 단위 조회·저장

ViewModel에는 DB 컬럼 구조를 노출하지 않는다.

```csharp
using System.Threading.Tasks;

public interface IScrapMixRepository
{
    Task<ScrapMix?> LoadAsync(string scrapNo);
    Task SaveAsync(ScrapMix mix);
}
```

구현 예시는 다음과 같다.

```csharp
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

public sealed class ScrapMixRepository : IScrapMixRepository
{
    private readonly Func<DbConnection> _createConnection;

    public ScrapMixRepository(Func<DbConnection> createConnection)
    {
        _createConnection = createConnection;
    }

    public async Task<ScrapMix?> LoadAsync(string scrapNo)
    {
        using var connection = _createConnection();
        await connection.OpenAsync();

        const string sql = """
            SELECT *
            FROM [te_scrap_mix]
            WHERE [sScrapno_001] = @ScrapNo
            """;

        var row = await connection.QuerySingleOrDefaultAsync(
            sql, new { ScrapNo = scrapNo });

        if (row is null)
            return null;

        return ScrapMixMapper.FromRow(
            (IDictionary<string, object>)row);
    }

    public async Task SaveAsync(ScrapMix mix)
    {
        var columns = ScrapMixMapper.ToColumns(mix);

        // 컬럼명은 Mapper가 정의한 이름만 사용한다.
        // 데이터 값은 모두 SQL 파라미터로 전달한다.
        string assignments = string.Join(
            ", ",
            columns.Keys
                .Where(name => name != "sScrapno_001")
                .Select(name => $"[{name}] = @{name}"));

        string sql = $"""
            UPDATE [te_scrap_mix]
            SET {assignments}
            WHERE [sScrapno_001] = @sScrapno_001
            """;

        var parameters = new DynamicParameters();

        foreach (var pair in columns)
            parameters.Add(pair.Key, pair.Value);

        using var connection = _createConnection();
        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction();

        int affected = await connection.ExecuteAsync(
            sql, parameters, transaction);

        if (affected != 1)
        {
            transaction.Rollback();

            throw new InvalidOperationException(
                $"저장 대상은 1행이어야 합니다. 실제: {affected}행");
        }

        transaction.Commit();
    }
}
```

`Func<DbConnection>`은 호출할 때마다 새 연결 객체를 반환하도록 구성한다. 연결은 Repository에서 열고 해제한다. 생성자 주입 시 실제 DB 공급자의 연결 객체와 연결 문자열을 조립한다.

이 저장 메서드는 기존 배합 수정용이다. 신규 배합 등록은 `InsertAsync()`로 분리한다. 실제 DB의 트리거 및 영향 행 수 반환 설정에 따라 `affected` 검사 방식도 검토해야 한다.

## 4. ViewModel: 현재 배합과 장입 선택

ViewModel은 반복 컬럼명을 다루지 않고 조회·저장 명령과 화면 상태만 관리한다.

```csharp
using System;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

public partial class ScrapMixViewModel : ObservableObject
{
    private readonly IScrapMixRepository _repository;

    [ObservableProperty]
    private string searchScrapNo = "";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private ScrapMix? currentMix;

    [ObservableProperty]
    private ScrapCharge? selectedCharge;

    [ObservableProperty]
    private string message = "";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoadCommand))]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private bool isBusy;

    public ScrapMixViewModel(IScrapMixRepository repository)
    {
        _repository = repository;
    }

    partial void OnCurrentMixChanged(ScrapMix? value)
    {
        SelectedCharge = value?.Charges.FirstOrDefault();
    }

    private bool CanLoad() => !IsBusy;

    private bool CanSave() => !IsBusy && CurrentMix is not null;

    [RelayCommand(CanExecute = nameof(CanLoad))]
    private async Task LoadAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchScrapNo))
        {
            Message = "배합번호를 입력하세요.";
            return;
        }

        try
        {
            IsBusy = true;

            CurrentMix = await _repository.LoadAsync(
                SearchScrapNo.Trim());

            Message = CurrentMix is null
                ? "해당 배합이 없습니다."
                : "조회했습니다.";
        }
        catch (Exception)
        {
            // 실제 구현에서는 예외 상세를 로그에 기록한다.
            Message = "조회에 실패했습니다.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        var mix = CurrentMix;
        if (mix is null)
            return;

        try
        {
            IsBusy = true;

            await _repository.SaveAsync(mix);
            Message = "저장했습니다.";
        }
        catch (Exception)
        {
            // 실제 구현에서는 예외 상세를 로그에 기록한다.
            Message = "저장에 실패했습니다.";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
```

`[RelayCommand]`는 `LoadAsync()`와 `SaveAsync()`에 대해 각각 `LoadCommand`, `SaveCommand`를 생성한다. `IsBusy` 변경 시 명령 실행 가능 여부를 다시 평가한다.

## 5. View: 선택한 장입의 원료 목록 표시

다음은 Window 또는 UserControl 내부에 배치할 XAML이다.

```xml
<StackPanel Margin="20">
    <TextBlock Text="배합번호" />

    <TextBox Text="{Binding SearchScrapNo,
                           UpdateSourceTrigger=PropertyChanged}" />

    <Button Content="조회"
            Command="{Binding LoadCommand}" />

    <TextBlock Text="{Binding CurrentMix.ScrapNo}"
               Margin="0,12,0,0" />

    <TextBlock Text="장입차수" />

    <ComboBox ItemsSource="{Binding CurrentMix.Charges}"
              SelectedItem="{Binding SelectedCharge, Mode=TwoWay}"
              DisplayMemberPath="ChargeNo" />

    <DataGrid ItemsSource="{Binding SelectedCharge.Materials}"
              AutoGenerateColumns="False"
              CanUserAddRows="False"
              CanUserDeleteRows="False"
              Height="300">
        <DataGrid.Columns>
            <DataGridTextColumn Header="순번"
                                IsReadOnly="True"
                                Binding="{Binding Sequence}" />

            <DataGridTextColumn Header="번호"
                Binding="{Binding ScrapItemNo,
                                  UpdateSourceTrigger=PropertyChanged}" />

            <DataGridTextColumn Header="구분"
                Binding="{Binding Category,
                                  UpdateSourceTrigger=PropertyChanged}" />

            <DataGridTextColumn Header="CODE"
                Binding="{Binding Code,
                                  UpdateSourceTrigger=PropertyChanged,
                                  ValidatesOnExceptions=True}" />

            <DataGridTextColumn Header="품명"
                Binding="{Binding Name,
                                  UpdateSourceTrigger=PropertyChanged}" />

            <DataGridTextColumn Header="예정량"
                Binding="{Binding PlannedAmount,
                                  UpdateSourceTrigger=PropertyChanged,
                                  ValidatesOnExceptions=True}" />
        </DataGrid.Columns>
    </DataGrid>

    <TextBlock Text="비고" />

    <TextBox Text="{Binding CurrentMix.Remark,
                           UpdateSourceTrigger=PropertyChanged}" />

    <Button Content="저장"
            Command="{Binding SaveCommand}" />

    <TextBlock Text="{Binding Message}" />
</StackPanel>
```

`DataGrid`의 각 행에서는 해당 `ScrapMaterial`이 바인딩 대상이 된다. 따라서 행 내부에서는 `CurrentMix` 등의 경로 없이 `Name`, `PlannedAmount`로 접근한다.

View는 생성자로 전달받은 ViewModel을 `DataContext`에 연결한다.

```csharp
using System.Windows;

public partial class ScrapMixView : Window
{
    public ScrapMixView(ScrapMixViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
```

앱 시작 지점이나 DI 컨테이너에서 `ScrapMixRepository → ScrapMixViewModel → ScrapMixView` 순서로 객체를 조립한다. 앱 시작 코드에서 창을 직접 생성한다면 `App.xaml`의 `StartupUri`와 중복되지 않도록 구성한다.

## 데이터 흐름

```text
조회
 View.LoadCommand
   → ViewModel
   → Repository.LoadAsync(배합번호)
   → Mapper.FromRow(DB 행)
   → CurrentMix 교체
   → SelectedCharge를 첫 장입으로 설정
   → 바인딩된 화면 갱신

장입 선택
 ComboBox
   → SelectedCharge 변경
   → 해당 장입의 Materials를 DataGrid에 표시

원료 수정
 DataGrid 셀 입력
   → ScrapMaterial 속성 변경
   → Model의 속성 변경 알림
   → 메모리 데이터 갱신

저장
 View.SaveCommand
   → ViewModel
   → Repository.SaveAsync(CurrentMix)
   → Mapper.ToColumns(Model)
   → 파라미터화된 UPDATE 실행
   → DB 반영 및 결과 메시지 갱신
```

Binding과 속성 변경 알림 자체는 DB 저장을 실행하지 않는다. 저장 명령을 실행해야 DB에 반영된다.

## 실무 적용 시 남은 사항

- 실제 테이블명, 기본키, 자료형, 문자열 길이 및 NULL 정책을 확인한다.
- 입력 검증 오류가 남아 있으면 저장을 막는다. 숫자 변환 실패 시 화면 입력과 Model 값이 다를 수 있으므로 오류 표시만으로 충분하지 않다.
- 예정량 변경 시 장입 TOTAL과 장입합계를 어떻게 계산할지 업무 규칙을 확정한다. 현재 예시는 합계를 자동 갱신하지 않는다.
- 저장 중 편집을 허용할지 정하고, 필요하면 편집 영역을 비활성화하거나 저장용 스냅샷을 사용한다.
- 수정 중 다른 배합을 조회할 때 변경 내용 처리 정책을 정한다.
- 실제 예외 상세는 로그로 남기고, 화면 메시지는 사용자에게 필요한 내용으로 제공한다.
- 신규 등록, 배합 복사, 삭제는 별도 명령과 Repository 메서드로 확장한다.
- 동시 수정이 가능하면 버전 컬럼 등을 사용한 충돌 검사를 추가한다.
- 실제 적용 시 반복 컬럼 411개의 매핑과 읽기·쓰기 왕복 결과를 검증한다.

## Template Method / Factory Method 적용 판단

이번 구조의 핵심은 반복 컬럼을 계층형 Model로 변환하는 것이다. 우선 Repository 인터페이스, 생성자 주입, Mapper 분리로 구현할 수 있다.

| 필요 조건 | 적용 방식 |
|---|---|
| 여러 Repository의 처리 순서가 같고 SQL·매핑만 다름 | Template Method 검토 |
| 운영용·테스트용 Repository 교체 | 인터페이스와 생성자 주입 |
| 상위 생성 흐름에서 하위 클래스가 Repository 종류 결정 | Factory Method 검토 |

반복 컬럼이 많다는 이유만으로 Repository 상속 구조나 Factory Method를 추가할 필요는 없다. DB를 실제 마스터·디테일 테이블로 분리한다면 Model과 ViewModel의 형태는 유지하고, Repository를 여러 테이블 조회 및 트랜잭션 저장 방식으로 변경할 수 있다.
