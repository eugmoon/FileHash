using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using FileHash.Data;
using System;
using System.Linq;

namespace FileHash.Models
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new FileHashContext(
                serviceProvider.GetRequiredService<
                    DbContextOptions<FileHashContext>>()))
            {
                // Look for any file hashes
                if (context.FileMeta.Any())
                {
                    return;   // DB has been seeded
                }

                context.FileMeta.AddRange(
                    new FileMeta
                    {
                        // angular-1.2.32/angular.js
                        Filename = "angular.js",
                        Hash = "7a92c4a8a3d3a21742500d2f574cf8d9",
                        HashType = "md5",
                        Size = "791536 bytes",
                        Last = DateTime.Parse("10/11/2016 14:53:54")
                    },

                    new FileMeta
                    {
                        // angular-1.2.32/version.json
                        Filename = "version.json",
                        Hash = "5bf3ec89b666fe26d4fde923b533af0b",
                        HashType = "md5",
                        Size = "314 bytes",
                        Last = DateTime.Parse("10/11/2016 14:53:54")
                    },

                    new FileMeta
                    {
                        // Documents/DieRoller/DieRoller/Info.plist
                        Filename = "Info.plist",
                        Hash = "95c71dbe598de6169e927fbf5c579175da90d355",
                        HashType = "sha1",
                        Size = "1463 bytes",
                        Last = DateTime.Parse("12/18/2018 01:57:23")
                    },

                    new FileMeta
                    {
                        // Documents/DieRoller/DieRoller/AppDelegate.swift
                        Filename = "AppDelegate.swift",
                        Hash = "28c66f140e30cdf2622ace55cc82d6a5f6a07328",
                        HashType = "sha1",
                        Size = "2173 bytes",
                        Last = DateTime.Parse("12/18/2018 01:57:23")
                    },

                    new FileMeta
                    {
                        // Documents/Swift.old/CalculatorBrain/CalculatorBrain/ViewController.swift
                        Filename = "ViewController.swift",
                        Hash = "905810e660ebf99507d852aacc78d74b4f961627",
                        HashType = "sha1",
                        Size = "2315 bytes",
                        Last = DateTime.Parse("06/26/2017 02:31:14")
                    },

                    new FileMeta
                    {
                        // Documents/Swift.old/CalculatorBrain/CalculatorBrain/CalculatorBrain.swift
                        Filename = "CalculatorBrain.swift",
                        Hash = "a140dd5c632dd140bc0f76c5cf2127c3",
                        HashType = "md5",
                        Size = "5831 bytes",
                        Last = DateTime.Parse("06/26/2017 02:32:20")
                    }
                );
                context.SaveChanges();
            }
        }
    }
}