namespace Codegen.Python

module Parsing =
  open Ballerina.StdLib.StringBuilder
  open Codegen.Python.Syntax

  let getFieldCode: StringBuilder =
    StringBuilder.Many
      [ StringBuilder.One """def get_field(data: Json, field_name: str) -> Sum[ParsingError, Json]:"""
        StringBuilder.Many
          [ StringBuilder.One """match data:"""
            StringBuilder.Many
              [ StringBuilder.One """case dict():"""
                StringBuilder.Many
                  [ StringBuilder.One """if field_name in data:"""
                    StringBuilder.One """return Sum.right(data[field_name])"""
                    |> StringBuilder.Map indent
                    StringBuilder.One """return Sum.left(ParsingError.single(f"Field {field_name} not found"))""" ]
                |> StringBuilder.Map indent
                StringBuilder.One """case _:"""
                StringBuilder.One """return Sum.left(ParsingError.single(f"Expected dict, got {data}"))"""
                |> StringBuilder.Map indent ]
            |> StringBuilder.Map indent ]
        |> StringBuilder.Map indent ]
