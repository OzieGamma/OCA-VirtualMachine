// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.fs" company="Oswald Maskens">
//   Copyright 2014 Oswald Maskens
//   
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//   
//       http://www.apache.org/licenses/LICENSE-2.0
//   
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
module OCA.VirtualMachine.Program

open OCA.AsmLib

let reg r = Register.fromFriendly r |> Attempt.get
let bp = reg "bp"
let sp = reg "sp"
let ra = reg "ra"
let basePointer = 1000u
let memory = Array.create 4000 0u
let registers = Array.create 126 0u

let imm16value imm = 
    match imm with
    | Imm16.Value immV -> uint32 immV
    | _ -> failwithf "Unsupported imm value %A" imm

let readRegister r = 
    match r with
    | Reg v -> 
        match v with
        | 0us -> 0u
        | 1us -> 1u
        | _ -> registers.[(int v) - 2]

let writeRegister r value = 
    match r with
    | Reg v -> 
        match v with
        | 0us -> v |> ignore
        | 1us -> v |> ignore
        | _ -> registers.[(int v) - 2] <- value

let readMemory imm r = memory.[int ((imm16value imm) + (readRegister r) - basePointer)]
let writeMemory imm r v = memory.[int ((imm16value imm) + (readRegister r) - basePointer)] <- v

let readString r = 
    let length = readMemory (Imm16.Value 0us) r
    Array.init (int length) (fun i -> readMemory (Imm16.Value(uint16 i + 1us)) r |> byte) |> System.Text.ASCIIEncoding.ASCII.GetString

let execExit() : unit = 
    let exitCode = readRegister (reg "s0")
    printfn "Program exited with exit code: %i" exitCode

let execPrint() : unit = 
    let str = readString (reg "s0")
    printfn "Program-output: %s" str

let rec exec address : unit = 
    if address < basePointer then 
        match address with
        | (* exit *) 0x0002u -> execExit()
        | (* print *) 0x0003u -> 
            execPrint()
            exec (readRegister ra)
        | _ -> failwithf "Unknown kernel address %A" address
    else 
        let instr = 
            memory.[int (address - basePointer)]
            |> Instr.fromBin
            |> Attempt.get
        
        let inline execOp rS rA rB f = writeRegister rS (f (readRegister rA) (readRegister rB))
        match instr with
        | Ldw(rS, imm, rA) -> 
            writeRegister rS (readMemory imm rA)
            exec (address + 1u)
        | Stw(rB, imm, rA) -> 
            writeMemory imm rA (readRegister rB)
            exec (address + 1u)
        | Set(rS, imm) -> 
            writeRegister rS (imm16value imm)
            exec (address + 1u)
        | Add(rS, rA, rB) -> 
            execOp rS rA rB (fun a b -> a + b)
            exec (address + 1u)
        | Sub(rS, rA, rB) -> 
            execOp rS rA rB (fun a b -> a - b)
            exec (address + 1u)
        | And(rS, rA, rB) -> 
            execOp rS rA rB (fun a b -> a &&& b)
            exec (address + 1u)
        | Or(rS, rA, rB) -> 
            execOp rS rA rB (fun a b -> a ||| b)
            exec (address + 1u)
        | Xor(rS, rA, rB) -> 
            execOp rS rA rB (fun a b -> a ^^^ b)
            exec (address + 1u)
        | Nor(rS, rA, rB) -> 
            execOp rS rA rB (fun a b -> ~~~(a ||| b))
            exec (address + 1u)
        | Sll(rS, rA, rB) -> 
            execOp rS rA rB (fun a b -> a <<< (int (b % 32u)))
            exec (address + 1u)
        | Srl(rS, rA, rB) -> 
            execOp rS rA rB (fun a b -> a >>> (int (b % 32u)))
            exec (address + 1u)
        | Compge(rS, rA, rB) -> 
            execOp rS rA rB (fun a b -> 
                if (int a) >= (int b) then 1u
                else 0u)
            exec (address + 1u)
        | Complt(rS, rA, rB) -> 
            execOp rS rA rB (fun a b -> 
                if (int a) < (int b) then 1u
                else 0u)
            exec (address + 1u)
        | Compgeu(rS, rA, rB) -> 
            execOp rS rA rB (fun a b -> 
                if a >= b then 1u
                else 0u)
            exec (address + 1u)
        | Compltu(rS, rA, rB) -> 
            execOp rS rA rB (fun a b -> 
                if a < b then 1u
                else 0u)
            exec (address + 1u)
        | Compne(rS, rA, rB) -> 
            execOp rS rA rB (fun a b -> 
                if a <> b then 1u
                else 0u)
            exec (address + 1u)
        | Compeq(rS, rA, rB) -> 
            execOp rS rA rB (fun a b -> 
                if a = b then 1u
                else 0u)
            exec (address + 1u)
        | Bnez(rA, imm) -> 
            if (readRegister rA) <> 0u then 
                exec (address + (imm16value imm))
                exec (address + 1u)
        | Beqz(rA, imm) -> 
            if (readRegister rA) = 0u then 
                exec (address + (imm16value imm))
                exec (address + 1u)
        | Calli imm -> 
            writeRegister ra (address + 1u)
            exec (address + (imm16value imm))
        | Callr rA -> 
            let newAddr = readRegister rA
            writeRegister ra (address + 1u)
            exec newAddr
        | _ -> failwithf "Unknown instruction %A" instr

(* VM startup code *)
let toUint32Array (bytes : byte []) = 
    if bytes.Length % 4 <> 0 then failwith "Invalid binary file."
    else 
        let words = new System.Collections.Generic.List<uint32>()
        for i in 0..4..(bytes.Length - 1) do
            words.Add(System.BitConverter.ToUInt32(bytes, i))
        words |> Array.ofSeq

[<EntryPoint>]
let main argv = 
    if argv.Length <> 1 then printfn "Invalid number of args %i" argv.Length
    else 
        let binData = System.IO.File.ReadAllBytes(argv.[0]) |> toUint32Array
        for i = 0 to binData.Length - 1 do
            memory.[i] <- binData.[i]
        writeRegister bp basePointer
        exec basePointer
        System.Console.Read() |> ignore
    0 // return an integer exit code
