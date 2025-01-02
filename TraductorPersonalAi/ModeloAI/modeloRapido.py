import sys
import torch
from transformers import MarianMTModel, MarianTokenizer

def load_model_and_tokenizer(model_path, device):
    model = MarianMTModel.from_pretrained(model_path).to(device)
    tokenizer = MarianTokenizer.from_pretrained(model_path)
    model = torch.compile(model)
    return model, tokenizer

def batch_translate(input_texts, model, tokenizer, device):
    inputs = tokenizer(input_texts, return_tensors="pt", padding=True, truncation=True).to(device)
    with torch.no_grad():
        translated = model.generate(**inputs)
    return [tokenizer.decode(t, skip_special_tokens=True) for t in translated]

if __name__ == "__main__":
    model_path = "D:/Programacion/Visual Studio/Modelo_AI/"
    device = torch.device("cuda" if torch.cuda.is_available() else "cpu")
    model, tokenizer = load_model_and_tokenizer(model_path, device)

    input_texts = sys.argv[1].split("|||")
    translations = batch_translate(input_texts, model, tokenizer, device)
    print("|||".join(translations))
