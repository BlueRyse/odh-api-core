﻿module RawQueryParser.TransformerTests

open Expecto
open Parser

[<Tests>]
let transfomerTests =
    testList "Transformer" [
        testList "Sorting" [
            test "Simple sort statement" {
                let expected = "data#>>'\{Detail,de,Title\}' ASC"
                let actual = Transformer.transformSort "Detail.de.Title"
                Expect.equal actual expected ""
            }
            test "Simple sort statement descending" {
                let expected = "data#>>'\{Detail,de,Title\}' DESC"
                let actual = Transformer.transformSort "-Detail.de.Title"
                Expect.equal actual expected ""
            }
            test "Combined sort statements" {
                let expected = "data#>>'\{Detail,de,Title\}' ASC, data#>>'\{Detail,de,Body\}' DESC"
                let actual = Transformer.transformSort "Detail.de.Title, -Detail.de.Body"
                Expect.equal actual expected ""
            }
        ]
        testList "Filtering" [
            test "Simple filter" {
                let expected = "data#>>'\{Active\}' = 'true'"
                let actual = Transformer.transformFilter "eq(Active, true)"
                Expect.equal actual expected ""
            }
            test "Simple stringy filter" {
                let expected = "data#>>'\{Type\}' = 'Wandern'"
                let actual = Transformer.transformFilter "eq(Type, 'Wandern')"
                Expect.equal actual expected ""
            }
            test "Simple AND filter" {
                let expected = "(data#>>'\{Geo,Altitude\}' >= '200' AND data#>>'\{Geo,Altitude\}' <= '400')"
                let actual = Transformer.transformFilter "and(ge(Geo.Altitude, 200), le(Geo.Altitude, 400))"
                Expect.equal actual expected ""
            }
            test "AND filter with multiple conditions" {
                let expected = "(data#>>'\{Active\}' = 'true' AND (data#>>'\{Geo,Altitude\}' >= '200' AND data#>>'\{Geo,Altitude\}' <= '400'))"
                let actual = Transformer.transformFilter "and(eq(Active, true), ge(Geo.Altitude, 200), le(Geo.Altitude, 400))"
                Expect.equal actual expected ""
            }
            test "AND filter with nested OR" {
                let expected = "(data#>>'\{Active\}' = 'true' OR (data#>>'\{Geo,Altitude\}' >= '200' AND data#>>'\{Geo,Altitude\}' <= '400'))"
                let actual = Transformer.transformFilter "or(eq(Active, true), and(ge(Geo.Altitude, 200), le(Geo.Altitude, 400)))"
                Expect.equal actual expected ""
            }
        ]
    ]
