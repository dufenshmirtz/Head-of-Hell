import argparse
import os
import subprocess
import sys


def run_step(cmd, step_name):
    print(f"\n{'=' * 70}")
    print(f"RUNNING: {step_name}")
    print(f"COMMAND: {' '.join(cmd)}")
    print(f"{'=' * 70}")
    subprocess.run(cmd, check=True)


def file_exists(path):
    return os.path.isfile(path)


def get_default_output_root():
    documents_dir = os.path.join(os.path.expanduser("~"), "Documents")
    return os.path.join(documents_dir, "My Games", "Head of Hell", "PipelineOutputs")


def main():
    parser = argparse.ArgumentParser(
        description="Run the telemetry analysis pipeline for the fighting game project."
    )

    output_root = get_default_output_root()
    documents_dir = os.path.join(os.path.expanduser("~"), "Documents")
    parser.add_argument("--python", default=sys.executable, help="Python executable to use")
    parser.add_argument(
        "--telemetry-dir",
        default=os.path.join(documents_dir, "My Games", "Head of Hell", "Telemetry", "Build"),
        help="Telemetry folder path"
    )
    parser.add_argument(
        "--out-dir",
        default=os.path.join(output_root, "out"),
        help="Extractor output folder"
    )
    parser.add_argument(
        "--elo-out-dir",
        default=os.path.join(output_root, "out_elo"),
        help="Elo output folder"
    )
    parser.add_argument(
        "--style-out-dir",
        default=os.path.join(output_root, "style_out"),
        help="Clustering output folder"
    )
    parser.add_argument(
        "--style-maps-dir",
        default=os.path.join(output_root, "style_maps"),
        help="Round style map output folder"
    )
    parser.add_argument(
        "--player-style-maps-dir",
        default=os.path.join(output_root, "player_style_maps"),
        help="Player style map output folder"
    )

    parser.add_argument("--with-check", action="store_true", help="Also run telemetry_check.py first")
    parser.add_argument("--with-player-style-prof", action="store_true", help="Also run player_style_prof.py at the end")

    args = parser.parse_args()

    py = args.python
    steps = []

    print(f"\nUsing telemetry folder: {args.telemetry_dir}")
    print(f"Using output root: {output_root}")

    if args.with_check and file_exists("telemetry_check.py"):
        steps.append((
            [py, "telemetry_check.py", "--telemetry-dir", args.telemetry_dir],
            "telemetry_check.py"
        ))

    if file_exists("extractor_upgraded.py"):
        steps.append((
            [
                py, "extractor_upgraded.py",
                "--telemetry-dir", args.telemetry_dir,
                "--out-dir", args.out_dir
            ],
            "extractor_upgraded.py"
        ))
    else:
        raise FileNotFoundError("Missing extractor_upgraded.py")

    if file_exists("elo_from_telemetry.py"):
        steps.append((
            [
                py, "elo_from_telemetry.py",
                "--telemetry-dir", args.telemetry_dir,
                "--out-dir", args.elo_out_dir
            ],
            "elo_from_telemetry.py"
        ))
    else:
        raise FileNotFoundError("Missing elo_from_telemetry.py")

    if file_exists("style_clustering_pipeline.py"):
        steps.append((
            [
                py, "style_clustering_pipeline.py",
                "--round-csv", os.path.join(args.out_dir, "dataset_round_level.csv"),
                "--elo-csv", os.path.join(args.elo_out_dir, "elo_overall.csv"),
                "--outdir", args.style_out_dir
            ],
            "style_clustering_pipeline.py"
        ))
    else:
        raise FileNotFoundError("Missing style_clustering_pipeline.py")

    if file_exists("round_style_map.py"):
        steps.append((
            [
                py, "round_style_map.py",
                "--input", os.path.join(args.out_dir, "dataset_round_level.csv"),
                "--output-dir", args.style_maps_dir
            ],
            "round_style_map.py"
        ))
    else:
        print("\n[WARN] round_style_map.py not found -> skipping round style maps")

    if file_exists("export_profile_analysis.py"):
        steps.append((
            [
                py, "export_profile_analysis.py",
                "--input", os.path.join(args.out_dir, "dataset_profile_level.csv"),
                "--output-dir", os.path.join(args.out_dir, "profile_analysis"),

                # Κρατάμε αυτό όπως είναι προς το παρόν για να μη ρισκάρουμε το refresh flow
                "--unity-output", os.path.join("out", "profile_analysis", "profile_analysis.json"),
            ],
            "export_profile_analysis.py"
        ))
    else:
        raise FileNotFoundError("Missing export_profile_analysis.py")

    if file_exists("player_style_map.py"):
        steps.append((
            [
                py, "player_style_map.py",
                "--input", os.path.join(args.out_dir, "profile_analysis", "profile_analysis.json"),
                "--output-dir", args.player_style_maps_dir
            ],
            "player_style_map.py"
        ))
    else:
        print("\n[WARN] player_style_map.py not found -> skipping player style maps")

    if args.with_player_style_prof:
        if file_exists("player_style_prof.py"):
            steps.append((
                [py, "player_style_prof.py"],
                "player_style_prof.py"
            ))
        else:
            print("\n[WARN] player_style_prof.py not found -> skipping")

    for cmd, name in steps:
        run_step(cmd, name)

    print(f"\n{'=' * 70}")
    print("PIPELINE COMPLETED")
    print(f"{'=' * 70}")


if __name__ == "__main__":
    main()