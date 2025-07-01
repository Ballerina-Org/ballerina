namespace Ballerina

module Fun =

  type U<'s> = 's -> 's

  let (>>?) (f: Option<U<'a>>) (g: Option<U<'a>>) : Option<U<'a>> =
    match f, g with
    | Some f, Some g -> Some(fun x -> g (f (x)))
    | None, Some _ -> g
    | Some _, None -> f
    | _ -> None

  type Updater<'s> = U<'s>

  let replaceWith (v: 'a) : U<'a> = fun _ -> v
  let curry f x y = f (x, y)
  let uncurry f (x, y) = f x y
