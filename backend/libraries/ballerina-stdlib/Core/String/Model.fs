namespace Ballerina.StdLib

module String =

  open System
  open System.Text
  open System.Text.RegularExpressions
  open System.Security.Cryptography

  // let private join (c:string) (s:string seq) = String.Join(c,s)
  let private join' (c: char) (s: string seq) = String.Join(c, s)

  let hashSha256 (input: string) =
    input |> Encoding.UTF8.GetBytes |> SHA256.HashData |> Convert.ToHexString

  type String with
    member self.ReasonablyClamped =
      Regex.Replace(self.Substring(0, min self.Length 250).ReplaceLineEndings(" "), " +", " ")
      + "..."

    static member append (s2: string) s1 = s1 + s2
    static member appendNewline s2 s1 = s1 + "\n" + s2

    static member ToPascalCase (separators: char array) (self: String) =
      let elements = self.Split separators
      let elements = elements |> Seq.map String.ToFirstUpper
      elements |> Seq.fold (+) String.Empty

    member self.ToFirstUpper =
      if self |> String.IsNullOrEmpty then
        self
      else
        String.Concat(self[0].ToString().ToUpper(), self.AsSpan(1))

    static member ToFirstUpper(self: String) = self.ToFirstUpper
    static member JoinSeq (separator: char) (self: string seq) = join' separator self
    static member join (separator: string) (self: string seq) = String.Join(separator, self)

  type NonEmptyString =
    private
    | NonEmptyString of string

    static member TryCreate(s: string) =
      match s with
      | "" -> None
      | _ -> NonEmptyString s |> Some

    static member Create (head: char) (tail: string) = NonEmptyString(string head + tail)

    static member AsString(NonEmptyString s) = s

  let (|NonEmptyString|) (NonEmptyString string) = string
