namespace Ballerina

type Range =
  { skip: int
    take: int }

  static member Default(skip: int, take: int) : Range = { skip = skip; take = take }
  static member WithinReason(skip: int, take: int) = { skip = skip; take = min take 100 }
