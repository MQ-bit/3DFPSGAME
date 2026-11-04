using System.Text.RegularExpressions;

// 계정(아이디, 비밀번호)이 지켜야 할 규칙 모음
public class AccountSpecification
{
    // 이메일: (영문, 숫자, ., _, -)@(영문, 숫자, -)(.영문, 숫자, -)를 1번 이상
    // 예) abc.def@gmail.com, user_01@mail.co.kr
    private static readonly Regex EmailRegex = new Regex(@"^[A-Za-z0-9._-]+@[A-Za-z0-9-]+(\.[A-Za-z0-9-]+)+$");

    // 비밀번호: 대문자, 소문자, 숫자, 기호를 각각 1개 이상 포함한 8~30자 (ASCII 문자만, 공백 불가)
    // 예) Abcd123!, MyP@ssw0rd
    private static readonly Regex PasswordRegex = new Regex(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*[0-9])(?=.*[^A-Za-z0-9])[!-~]{8,30}$");

    public string IdErrorMessage => "아이디는 이메일 형식이어야 합니다.";
    public string PasswordErrorMessage => "비밀번호는 대문자, 소문자, 숫자, 기호를 모두 포함한 8~30자여야 합니다.";

    public bool IsIdSatisfiedBy(string id)
    {
        return EmailRegex.IsMatch(id);
    }

    public bool IsPasswordSatisfiedBy(string password)
    {
        return PasswordRegex.IsMatch(password);
    }
}
